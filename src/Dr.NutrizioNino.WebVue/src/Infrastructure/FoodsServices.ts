import axios from 'axios'

import type { FoodDashboardDto } from '../Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '../Interfaces/foods/FoodDto'
import type { ApiResponseSingeDto } from '@/Interfaces/ApiResponseSingeDto'
import type { ApiResponseMultipleDto } from '@/Interfaces/ApiResponseMultipleDto'

export class FoodsService {
  private async dashboardFetchData(): Promise<ApiResponseMultipleDto<FoodDashboardDto>> {
    try {
      const response = await axios.get('https://localhost:44360/foods/dashboard')
      return response.data
    } catch (error) {
      console.error(error)
      throw error
    }
  }

  public async GetDashboardRow(id: string): Promise<ApiResponseSingeDto<FoodDashboardDto>> {
    const data = await this.dashboardRowFetchData(id)
    return data
  }

  private async dashboardRowFetchData(id: string): Promise<ApiResponseSingeDto<FoodDashboardDto>> {
    try {
      const response = await axios.get(`https://localhost:44360/foods/dashboard/${id}`)
      return response.data
    } catch (error) {
      console.error(error)
      throw error
    }
  }

  public async GetDashboard(): Promise<ApiResponseMultipleDto<FoodDashboardDto>> {
    const data = await this.dashboardFetchData()
    return data
  }

  private async foodFactoryFetchData(): Promise<ApiResponseMultipleDto<FoodDto>> {
    try {
      const response = await axios.get('https://localhost:44360/foods/getnewfood')
      return response.data
    } catch (error) {
      console.error(error)
      throw error
    }
  }

  public async FoodFactoryGetNew(): Promise<ApiResponseMultipleDto<FoodDto>> {
    const data = await this.foodFactoryFetchData()
    return data
  }
}
