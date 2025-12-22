import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
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
  CircularProgress,
} from '@mui/material'
import { Save as SaveIcon, Cancel as CancelIcon } from '@mui/icons-material'
import { toast } from 'react-toastify'
import { ruuviTagsApi } from '../api/ruuviTags'
import { UpdateRuuviTagRequest } from '../types'

function EditTag() {
  const navigate = useNavigate()
  const { macAddress } = useParams<{ macAddress: string }>()
  const queryClient = useQueryClient()
  const [error, setError] = useState<string | null>(null)

  const { data: tag, isLoading } = useQuery({
    queryKey: ['ruuviTag', macAddress],
    queryFn: () => ruuviTagsApi.getById(macAddress!),
    enabled: !!macAddress,
  })

  const {
    control,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<UpdateRuuviTagRequest>()

  useEffect(() => {
    if (tag) {
      reset({
        name: tag.name,
        dataSavingInterval: tag.dataSavingInterval,
        calculateAverages: tag.calculateAverages,
        storeAcceleration: tag.storeAcceleration,
        discardMinMaxValues: tag.discardMinMaxValues,
        allowHttp: tag.allowHttp,
      })
    }
  }, [tag, reset])

  const updateMutation = useMutation({
    mutationFn: (data: UpdateRuuviTagRequest) =>
      ruuviTagsApi.update(macAddress!, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ruuviTags'] })
      queryClient.invalidateQueries({ queryKey: ['ruuviTag', macAddress] })
      toast.success('Tag updated successfully')
      navigate('/tags')
    },
    onError: (error: any) => {
      const message = error.response?.data?.error || 'Failed to update tag'
      setError(message)
      toast.error(message)
    },
  })

  const onSubmit = (data: UpdateRuuviTagRequest) => {
    setError(null)
    updateMutation.mutate(data)
  }

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    )
  }

  if (!tag) {
    return (
      <Alert severity="error">
        Tag not found
      </Alert>
    )
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom sx={{ mb: 4 }}>
        Edit Ruuvi Tag
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
              <Grid item xs={12}>
                <TextField
                  label="MAC Address"
                  value={macAddress}
                  fullWidth
                  disabled
                  helperText="MAC address cannot be changed"
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
                      helperText={errors.name?.message}
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
                      control={<Switch {...field} checked={field.value || false} />}
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
                      control={<Switch {...field} checked={field.value || false} />}
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
                      control={<Switch {...field} checked={field.value || false} />}
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
                      control={<Switch {...field} checked={field.value || false} />}
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
                    disabled={updateMutation.isPending}
                  >
                    {updateMutation.isPending ? 'Saving...' : 'Save Changes'}
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

export default EditTag