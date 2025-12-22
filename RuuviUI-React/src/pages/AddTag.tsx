import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm, Controller } from 'react-hook-form'
import {
  Box,
  Button,
  Card,
  CardContent,
  TextField,
  Typography,
  Switch,
  FormControlLabel,
  Grid,
  Alert,
  InputAdornment,
} from '@mui/material'
import { Save as SaveIcon, Cancel as CancelIcon } from '@mui/icons-material'
import { toast } from 'react-toastify'
import { ruuviTagsApi } from '../api/ruuviTags'
import { CreateRuuviTagRequest } from '../types'

function AddTag() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [error, setError] = useState<string | null>(null)

  const {
    control,
    handleSubmit,
    formState: { errors },
    watch,
  } = useForm<CreateRuuviTagRequest>({
    defaultValues: {
      macAddress: '',
      name: '',
      dataSavingInterval: 60,
      calculateAverages: false,
      storeAcceleration: false,
      discardMinMaxValues: true,
      allowHttp: false,
    },
  })

  const createMutation = useMutation({
    mutationFn: ruuviTagsApi.create,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ruuviTags'] })
      toast.success('Tag created successfully')
      navigate('/tags')
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || 'Failed to create tag'
      setError(message)
      toast.error(message)
    },
  })

  const onSubmit = (data: CreateRuuviTagRequest) => {
    setError(null)
    // Format MAC address to uppercase with colons
    const formattedMac = data.macAddress
      .toUpperCase()
      .replace(/[^0-9A-F]/g, '')
      .match(/.{2}/g)
      ?.join(':')

    if (!formattedMac || formattedMac.length !== 17) {
      setError('Invalid MAC address format')
      return
    }

    createMutation.mutate({
      ...data,
      macAddress: formattedMac,
    })
  }

  const macAddress = watch('macAddress')
  const isValidMac = /^([0-9A-Fa-f]{2}[:-]?){5}([0-9A-Fa-f]{2})$/.test(macAddress)

  return (
    <Box>
      <Typography variant="h4" gutterBottom sx={{ mb: 4 }}>
        Add New Ruuvi Tag
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Card>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)}>
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <Controller
                  name="macAddress"
                  control={control}
                  rules={{
                    required: 'MAC address is required',
                    pattern: {
                      value: /^([0-9A-Fa-f]{2}[:-]?){5}([0-9A-Fa-f]{2})$/,
                      message: 'Invalid MAC address format',
                    },
                  }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="MAC Address"
                      fullWidth
                      error={!!errors.macAddress}
                      helperText={
                        errors.macAddress?.message ||
                        'Enter the MAC address of your Ruuvi tag (e.g., AA:BB:CC:DD:EE:FF)'
                      }
                      placeholder="AA:BB:CC:DD:EE:FF"
                      InputProps={{
                        endAdornment: (
                          <InputAdornment position="end">
                            {macAddress && (isValidMac ? '✓' : '✗')}
                          </InputAdornment>
                        ),
                      }}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} md={6}>
                <Controller
                  name="name"
                  control={control}
                  rules={{ required: 'Name is required' }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Tag Name"
                      fullWidth
                      error={!!errors.name}
                      helperText={
                        errors.name?.message ||
                        'Give your tag a friendly name (e.g., Living Room, Bedroom)'
                      }
                      placeholder="Living Room Sensor"
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12} md={6}>
                <Controller
                  name="dataSavingInterval"
                  control={control}
                  rules={{
                    required: 'Data saving interval is required',
                    min: { value: 1, message: 'Minimum interval is 1 second' },
                    max: { value: 3600, message: 'Maximum interval is 3600 seconds' },
                  }}
                  render={({ field }) => (
                    <TextField
                      {...field}
                      label="Data Saving Interval"
                      type="number"
                      fullWidth
                      error={!!errors.dataSavingInterval}
                      helperText={
                        errors.dataSavingInterval?.message ||
                        'How often to save measurements (in seconds)'
                      }
                      InputProps={{
                        endAdornment: <InputAdornment position="end">seconds</InputAdornment>,
                      }}
                    />
                  )}
                />
              </Grid>

              <Grid item xs={12}>
                <Typography variant="h6" gutterBottom>
                  Data Collection Settings
                </Typography>
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name="calculateAverages"
                  control={control}
                  render={({ field }) => (
                    <FormControlLabel
                      control={<Switch {...field} checked={field.value} />}
                      label="Calculate Averages"
                    />
                  )}
                />
                <Typography variant="caption" color="textSecondary" display="block">
                  Calculate average values for measurements
                </Typography>
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name="storeAcceleration"
                  control={control}
                  render={({ field }) => (
                    <FormControlLabel
                      control={<Switch {...field} checked={field.value} />}
                      label="Store Acceleration Data"
                    />
                  )}
                />
                <Typography variant="caption" color="textSecondary" display="block">
                  Store acceleration sensor data (X, Y, Z axes)
                </Typography>
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name="discardMinMaxValues"
                  control={control}
                  render={({ field }) => (
                    <FormControlLabel
                      control={<Switch {...field} checked={field.value} />}
                      label="Discard Min/Max Values"
                    />
                  )}
                />
                <Typography variant="caption" color="textSecondary" display="block">
                  Discard extreme values to reduce noise
                </Typography>
              </Grid>

              <Grid item xs={12} sm={6}>
                <Controller
                  name="allowHttp"
                  control={control}
                  render={({ field }) => (
                    <FormControlLabel
                      control={<Switch {...field} checked={field.value} />}
                      label="Allow HTTP Gateway"
                    />
                  )}
                />
                <Typography variant="caption" color="textSecondary" display="block">
                  Allow measurements through HTTP gateway
                </Typography>
              </Grid>

              <Grid item xs={12}>
                <Box display="flex" gap={2} justifyContent="flex-end">
                  <Button
                    variant="outlined"
                    startIcon={<CancelIcon />}
                    onClick={() => navigate('/tags')}
                  >
                    Cancel
                  </Button>
                  <Button
                    type="submit"
                    variant="contained"
                    startIcon={<SaveIcon />}
                    disabled={createMutation.isPending}
                  >
                    {createMutation.isPending ? 'Creating...' : 'Create Tag'}
                  </Button>
                </Box>
              </Grid>
            </Grid>
          </form>
        </CardContent>
      </Card>
    </Box>
  )
}

export default AddTag