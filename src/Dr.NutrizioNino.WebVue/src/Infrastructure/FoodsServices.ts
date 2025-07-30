import axios from 'axios'
import config from '@/config'

import type { FoodDashboardDto } from '../Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '../Interfaces/foods/FoodDto'
import type { ApiResponseSingeDto } from '@/Interfaces/ApiResponseSingeDto'
import type { ApiResponseMultipleDto } from '@/Interfaces/ApiResponseMultipleDto'

export class FoodsService {
  private async dashboardFetchData(): Promise<ApiResponseMultipleDto<FoodDashboardDto>> {
    try {
      const response = await axios.get(`${config.API_BASE_URL}/foods/dashboard`)
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
      const response = await axios.get(`${config.API_BASE_URL}/foods/dashboard/${id}`)
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

  private async foodFactoryFetchData(): Promise<ApiResponseSingeDto<FoodDto>> {
    try {
      const response = await axios.get(`${config.API_BASE_URL}/foods/getnewfood`)
      return response.data
    } catch (error) {
      console.error(error)
      throw error
    }
  }

  public async FoodFactoryGetNew(): Promise<ApiResponseSingeDto<FoodDto>> {
    const data = await this.foodFactoryFetchData()
    return data
  }
}
