import { useNavigate, useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import {
  Box,
  Button,
  Card,
  CardContent,
  Typography,
  Grid,
  CircularProgress,
  Alert,
  Chip,
  Divider,
  Paper,
} from '@mui/material'
import {
  Edit as EditIcon,
  ArrowBack as BackIcon,
  Thermostat,
  WaterDrop,
  Compress,
  Battery90,
  Speed,
  Router,
} from '@mui/icons-material'
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts'
import { format, parseISO } from 'date-fns'
import { ruuviTagsApi } from '../api/ruuviTags'

function TagDetails() {
  const navigate = useNavigate()
  const { macAddress } = useParams<{ macAddress: string }>()

  const { data: tag, isLoading: tagLoading } = useQuery({
    queryKey: ['ruuviTag', macAddress],
    queryFn: () => ruuviTagsApi.getById(macAddress!),
    enabled: !!macAddress,
  })

  const { data: measurements, isLoading: measurementsLoading } = useQuery({
    queryKey: ['ruuviMeasurements', macAddress],
    queryFn: () => ruuviTagsApi.getMeasurements(macAddress!),
    enabled: !!macAddress,
    refetchInterval: 30000, // Refresh every 30 seconds
  })

  if (tagLoading || measurementsLoading) {
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

  const latestMeasurement = measurements?.[0]

  const chartData = measurements?.slice(0, 100).reverse().map(m => ({
    time: format(parseISO(m.timestamp), 'HH:mm'),
    temperature: m.temperature,
    humidity: m.humidity,
    pressure: m.pressure ? m.pressure / 100 : null, // Convert Pa to hPa
  }))

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={4}>
        <Typography variant="h4">{tag.name}</Typography>
        <Box display="flex" gap={2}>
          <Button
            startIcon={<BackIcon />}
            onClick={() => navigate('/tags')}
            variant="outlined"
          >
            Back
          </Button>
          <Button
            startIcon={<EditIcon />}
            onClick={() => navigate(`/tags/${macAddress}/edit`)}
            variant="contained"
          >
            Edit
          </Button>
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Tag Information */}
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Tag Information
              </Typography>
              <Divider sx={{ my: 2 }} />
              <Box sx={{ '& > *': { mb: 2 } }}>
                <Box>
                  <Typography variant="caption" color="textSecondary">
                    MAC Address
                  </Typography>
                  <Typography variant="body1" sx={{ fontFamily: 'monospace' }}>
                    {tag.macAddress}
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="textSecondary">
                    Data Interval
                  </Typography>
                  <Typography variant="body1">
                    {tag.dataSavingInterval} seconds
                  </Typography>
                </Box>
                <Box>
                  <Typography variant="caption" color="textSecondary">
                    Settings
                  </Typography>
                  <Box display="flex" flexWrap="wrap" gap={1} mt={1}>
                    {tag.calculateAverages && (
                      <Chip label="Averages" size="small" color="primary" />
                    )}
                    {tag.storeAcceleration && (
                      <Chip label="Acceleration" size="small" color="primary" />
                    )}
                    {tag.discardMinMaxValues && (
                      <Chip label="Discard Min/Max" size="small" color="primary" />
                    )}
                    {tag.allowHttp && (
                      <Chip label="HTTP Gateway" size="small" color="primary" />
                    )}
                  </Box>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Current Measurements */}
        <Grid item xs={12} md={8}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Current Measurements
              </Typography>
              <Divider sx={{ my: 2 }} />
              {latestMeasurement ? (
                <Grid container spacing={2}>
                  <Grid item xs={6} sm={4}>
                    <Paper elevation={0} sx={{ p: 2, bgcolor: 'background.default' }}>
                      <Box display="flex" alignItems="center" gap={1}>
                        <Thermostat color="error" />
                        <Box>
                          <Typography variant="caption" color="textSecondary">
                            Temperature
                          </Typography>
                          <Typography variant="h6">
                            {latestMeasurement.temperature?.toFixed(1)}°C
                          </Typography>
                        </Box>
                      </Box>
                    </Paper>
                  </Grid>
                  <Grid item xs={6} sm={4}>
                    <Paper elevation={0} sx={{ p: 2, bgcolor: 'background.default' }}>
                      <Box display="flex" alignItems="center" gap={1}>
                        <WaterDrop color="info" />
                        <Box>
                          <Typography variant="caption" color="textSecondary">
                            Humidity
                          </Typography>
                          <Typography variant="h6">
                            {latestMeasurement.humidity?.toFixed(1)}%
                          </Typography>
                        </Box>
                      </Box>
                    </Paper>
                  </Grid>
                  <Grid item xs={6} sm={4}>
                    <Paper elevation={0} sx={{ p: 2, bgcolor: 'background.default' }}>
                      <Box display="flex" alignItems="center" gap={1}>
                        <Compress color="primary" />
                        <Box>
                          <Typography variant="caption" color="textSecondary">
                            Pressure
                          </Typography>
                          <Typography variant="h6">
                            {latestMeasurement.pressure
                              ? (latestMeasurement.pressure / 100).toFixed(1)
                              : 'N/A'}{' '}
                            hPa
                          </Typography>
                        </Box>
                      </Box>
                    </Paper>
                  </Grid>
                  <Grid item xs={6} sm={4}>
                    <Paper elevation={0} sx={{ p: 2, bgcolor: 'background.default' }}>
                      <Box display="flex" alignItems="center" gap={1}>
                        <Battery90 color="success" />
                        <Box>
                          <Typography variant="caption" color="textSecondary">
                            Battery
                          </Typography>
                          <Typography variant="h6">
                            {latestMeasurement.battery
                              ? `${latestMeasurement.battery.toFixed(0)} mV`
                              : 'N/A'}
                          </Typography>
                        </Box>
                      </Box>
                    </Paper>
                  </Grid>
                  <Grid item xs={6} sm={4}>
                    <Paper elevation={0} sx={{ p: 2, bgcolor: 'background.default' }}>
                      <Box display="flex" alignItems="center" gap={1}>
                        <Speed color="secondary" />
                        <Box>
                          <Typography variant="caption" color="textSecondary">
                            Movement
                          </Typography>
                          <Typography variant="h6">
                            {latestMeasurement.movementCounter ?? 'N/A'}
                          </Typography>
                        </Box>
                      </Box>
                    </Paper>
                  </Grid>
                  <Grid item xs={6} sm={4}>
                    <Paper elevation={0} sx={{ p: 2, bgcolor: 'background.default' }}>
                      <Box display="flex" alignItems="center" gap={1}>
                        <Router color="action" />
                        <Box>
                          <Typography variant="caption" color="textSecondary">
                            TX Power
                          </Typography>
                          <Typography variant="h6">
                            {latestMeasurement.txPower
                              ? `${latestMeasurement.txPower} dBm`
                              : 'N/A'}
                          </Typography>
                        </Box>
                      </Box>
                    </Paper>
                  </Grid>
                </Grid>
              ) : (
                <Alert severity="info">
                  No measurements available yet
                </Alert>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Historical Data Chart */}
        {chartData && chartData.length > 0 && (
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  Historical Data
                </Typography>
                <Divider sx={{ my: 2 }} />
                <ResponsiveContainer width="100%" height={400}>
                  <LineChart data={chartData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="time" />
                    <YAxis yAxisId="temp" orientation="left" domain={['auto', 'auto']} />
                    <YAxis yAxisId="humidity" orientation="right" domain={[0, 100]} />
                    <Tooltip />
                    <Legend />
                    <Line
                      yAxisId="temp"
                      type="monotone"
                      dataKey="temperature"
                      stroke="#ff5722"
                      name="Temperature (°C)"
                      strokeWidth={2}
                      dot={false}
                    />
                    <Line
                      yAxisId="humidity"
                      type="monotone"
                      dataKey="humidity"
                      stroke="#2196f3"
                      name="Humidity (%)"
                      strokeWidth={2}
                      dot={false}
                    />
                    <Line
                      yAxisId="temp"
                      type="monotone"
                      dataKey="pressure"
                      stroke="#4caf50"
                      name="Pressure (hPa)"
                      strokeWidth={2}
                      dot={false}
                    />
                  </LineChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </Grid>
        )}
      </Grid>
    </Box>
  )
}

export default TagDetails