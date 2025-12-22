export interface RuuviTag {
  macAddress: string
  name: string
  dataSavingInterval?: number
  calculateAverages?: boolean
  storeAcceleration?: boolean
  discardMinMaxValues?: boolean
  allowHttp?: boolean
  lastSeen?: string
  modificationTime?: string
}

export interface RuuviTagInfo {
  macAddress: string
  name: string
  lastSeen?: string
  modificationTime: string
}

export interface CreateRuuviTagRequest {
  macAddress: string
  name: string
  dataSavingInterval?: number
  calculateAverages?: boolean
  storeAcceleration?: boolean
  discardMinMaxValues?: boolean
  allowHttp?: boolean
}

export interface UpdateRuuviTagRequest {
  name?: string
  dataSavingInterval?: number
  calculateAverages?: boolean
  storeAcceleration?: boolean
  discardMinMaxValues?: boolean
  allowHttp?: boolean
}

export interface MeasurementDTO {
  timestamp: string
  temperature?: number
  humidity?: number
  pressure?: number
  battery?: number
  accelerationX?: number
  accelerationY?: number
  accelerationZ?: number
  movementCounter?: number
  measurementSequence?: number
  txPower?: number
}