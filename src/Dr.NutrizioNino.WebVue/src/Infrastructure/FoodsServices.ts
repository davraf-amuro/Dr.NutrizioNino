import axios from 'axios'
import config from '@/config'

import type { FoodDashboardDto } from '../Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '../Interfaces/foods/FoodDto'

export class FoodsService {
  public async GetDashboard(): Promise<FoodDashboardDto[]> {
    const data = await this.dashboardFetchData()
    return data
  }

  private async dashboardFetchData(): Promise<FoodDashboardDto[]> {
    try {
      const response = await axios.get(`${config.API_BASE_URL}/foods/dashboard`)
      return response.data
    } catch (error) {
      console.error(error)
      throw error
    }
  }

  public async GetDashboardRow(id: string): Promise<FoodDashboardDto> {
    const data = await this.dashboardRowFetchData(id)
    return data
  }

  private async dashboardRowFetchData(id: string): Promise<FoodDashboardDto> {
    try {
      const response = await axios.get(`${config.API_BASE_URL}/foods/dashboard/${id}`)
      return response.data
    } catch (error) {
      console.error(error)
      throw error
    }
  }

  public async FoodFactoryGetNew(): Promise<FoodDto> {
    const data = await this.foodFactoryFetchData()
    return data
  }

  private async foodFactoryFetchData(): Promise<FoodDto> {
    try {
      const response = await axios.get(`${config.API_BASE_URL}/foods/getnewfood`)
      return response.data
    } catch (error) {
      console.error(error)
      throw error
    }
  }

  public async PostNewFood(food: FoodDto): Promise<string> {
    const id = await this.postNewFoodData(food)
    return id
  }

  private async postNewFoodData(food: FoodDto): Promise<string> {
    try {
      const response = await axios.post(`${config.API_BASE_URL}/foods/create`, food)
      return response.data
    } catch (error) {
      console.error(error)
      throw error
    }
  }
}
