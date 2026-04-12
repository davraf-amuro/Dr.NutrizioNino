from fastapi import FastAPI
from pydantic import BaseModel
from transformers import Qwen2VLForConditionalGeneration, AutoProcessor
import base64, io, torch
from PIL import Image

app = FastAPI()

print("Caricamento modello Qwen2-VL-2B-Instruct...")

model_id = "Qwen/Qwen2-VL-2B-Instruct"

model = Qwen2VLForConditionalGeneration.from_pretrained(
    model_id,
    torch_dtype=torch.float32,
    device_map="cpu"
)
processor = AutoProcessor.from_pretrained(model_id)

print("Modello pronto!")


class Request(BaseModel):
    prompt: str
    image: str | None = None  # base64 dell'immagine, opzionale
    max_new_tokens: int = 1024


@app.post("/generate")
def generate(req: Request):
    pil_image = None
    if req.image:
        # Decodifica base64 manualmente — più robusto del data URI parsing
        padded = req.image + "=" * (-len(req.image) % 4)
        img_bytes = base64.b64decode(padded)
        pil_image = Image.open(io.BytesIO(img_bytes)).convert("RGB")

    if pil_image is not None:
        messages = [
            {
                "role": "user",
                "content": [
                    {"type": "image"},
                    {"type": "text", "text": req.prompt}
                ]
            }
        ]
    else:
        messages = [
            {"role": "user", "content": req.prompt}
        ]

    text = processor.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)

    inputs = processor(
        text=[text],
        images=[pil_image] if pil_image else None,
        padding=True,
        return_tensors="pt"
    )

    with torch.no_grad():
        generated_ids = model.generate(**inputs, max_new_tokens=req.max_new_tokens)

    generated_ids_trimmed = [
        out_ids[len(in_ids):]
        for in_ids, out_ids in zip(inputs.input_ids, generated_ids)
    ]
    output_text = processor.batch_decode(
        generated_ids_trimmed,
        skip_special_tokens=True,
        clean_up_tokenization_spaces=False
    )[0]

    return {"generated_text": output_text}
