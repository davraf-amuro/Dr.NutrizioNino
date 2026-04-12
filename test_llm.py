from transformers import Qwen2VLForConditionalGeneration, AutoProcessor
from qwen_vl_utils import process_vision_info
import torch

model_id = "Qwen/Qwen2-VL-2B-Instruct"

print("Caricamento modello...")
model = Qwen2VLForConditionalGeneration.from_pretrained(
    model_id,
    torch_dtype=torch.float32,
    device_map="cpu"
)
processor = AutoProcessor.from_pretrained(model_id)
print("Modello pronto!")

# Test testo puro
messages = [{"role": "user", "content": "Scrivi una frase su Gundam:"}]

text = processor.apply_chat_template(messages, tokenize=False, add_generation_prompt=True)
image_inputs, video_inputs = process_vision_info(messages)

inputs = processor(
    text=[text],
    images=image_inputs,
    videos=video_inputs,
    padding=True,
    return_tensors="pt"
)

with torch.no_grad():
    generated_ids = model.generate(**inputs, max_new_tokens=100)

generated_ids_trimmed = [
    out_ids[len(in_ids):]
    for in_ids, out_ids in zip(inputs.input_ids, generated_ids)
]
output = processor.batch_decode(generated_ids_trimmed, skip_special_tokens=True)[0]
print("Output:", output)
