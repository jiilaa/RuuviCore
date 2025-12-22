import axios from 'axios'
import {
  RuuviTag,
  RuuviTagInfo,
  CreateRuuviTagRequest,
  UpdateRuuviTagRequest,
  MeasurementDTO,
} from '../types'

const API_BASE_URL = import.meta.env.VITE_API_URL || ''

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

export const ruuviTagsApi = {
  getAll: async (): Promise<RuuviTagInfo[]> => {
    const { data } = await api.get('/api/ruuvitags')
    return data
  },

  getById: async (macAddress: string): Promise<RuuviTag> => {
    const { data } = await api.get(`/api/ruuvitags/${macAddress}`)
    return data
  },

  create: async (request: CreateRuuviTagRequest): Promise<RuuviTag> => {
    const { data } = await api.post('/api/ruuvitags', request)
    return data
  },

  update: async (macAddress: string, request: UpdateRuuviTagRequest): Promise<void> => {
    await api.put(`/api/ruuvitags/${macAddress}`, request)
  },

  delete: async (macAddress: string): Promise<void> => {
    await api.delete(`/api/ruuvitags/${macAddress}`)
  },

  getMeasurements: async (macAddress: string): Promise<MeasurementDTO[]> => {
    const { data } = await api.get(`/api/ruuvitags/${macAddress}/measurements`)
    return data
  },
}