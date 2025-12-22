import { useQuery } from '@tanstack/react-query'
import {
  Grid,
  Card,
  CardContent,
  Typography,
  Box,
  CircularProgress,
  Alert,
  Chip,
} from '@mui/material'
import {
  Bluetooth as BluetoothIcon,
  SignalCellularAlt as SignalIcon,
  Warning as WarningIcon,
  CheckCircle as CheckIcon,
} from '@mui/icons-material'
import { format, differenceInMinutes } from 'date-fns'
import { ruuviTagsApi } from '../api/ruuviTags'

function Dashboard() {
  const { data: tags, isLoading, error } = useQuery({
    queryKey: ['ruuviTags'],
    queryFn: ruuviTagsApi.getAll,
    refetchInterval: 30000, // Refresh every 30 seconds
  })

  if (isLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="400px">
        <CircularProgress />
      </Box>
    )
  }

  if (error) {
    return (
      <Alert severity="error">
        Failed to load Ruuvi tags. Please check your connection.
      </Alert>
    )
  }

  const activeTags = tags?.filter(tag => {
    if (!tag.lastSeen) return false
    const minutesAgo = differenceInMinutes(new Date(), new Date(tag.lastSeen))
    return minutesAgo < 5
  }) || []

  const inactiveTags = tags?.filter(tag => {
    if (!tag.lastSeen) return true
    const minutesAgo = differenceInMinutes(new Date(), new Date(tag.lastSeen))
    return minutesAgo >= 5
  }) || []

  return (
    <Box>
      <Typography variant="h4" gutterBottom sx={{ mb: 4 }}>
        Dashboard
      </Typography>

      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Total Tags
                  </Typography>
                  <Typography variant="h4">
                    {tags?.length || 0}
                  </Typography>
                </Box>
                <BluetoothIcon sx={{ fontSize: 40, color: 'primary.main', opacity: 0.3 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Active
                  </Typography>
                  <Typography variant="h4" sx={{ color: 'success.main' }}>
                    {activeTags.length}
                  </Typography>
                </Box>
                <CheckIcon sx={{ fontSize: 40, color: 'success.main', opacity: 0.3 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Inactive
                  </Typography>
                  <Typography variant="h4" sx={{ color: 'warning.main' }}>
                    {inactiveTags.length}
                  </Typography>
                </Box>
                <WarningIcon sx={{ fontSize: 40, color: 'warning.main', opacity: 0.3 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="textSecondary" gutterBottom>
                    Signal Quality
                  </Typography>
                  <Typography variant="h4">
                    {activeTags.length > 0
                      ? Math.round((activeTags.length / (tags?.length || 1)) * 100)
                      : 0}%
                  </Typography>
                </Box>
                <SignalIcon sx={{ fontSize: 40, color: 'info.main', opacity: 0.3 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Typography variant="h5" gutterBottom sx={{ mb: 2 }}>
        Recent Activity
      </Typography>

      {tags && tags.length > 0 ? (
        <Grid container spacing={2}>
          {tags.slice(0, 6).map((tag) => {
            const isActive = tag.lastSeen &&
              differenceInMinutes(new Date(), new Date(tag.lastSeen)) < 5

            return (
              <Grid item xs={12} md={6} key={tag.macAddress}>
                <Card>
                  <CardContent>
                    <Box display="flex" justifyContent="space-between" alignItems="center">
                      <Box>
                        <Typography variant="h6">{tag.name}</Typography>
                        <Typography variant="body2" color="textSecondary">
                          {tag.macAddress}
                        </Typography>
                        {tag.lastSeen && (
                          <Typography variant="caption" color="textSecondary">
                            Last seen: {format(new Date(tag.lastSeen), 'PPp')}
                          </Typography>
                        )}
                      </Box>
                      <Chip
                        label={isActive ? 'Online' : 'Offline'}
                        color={isActive ? 'success' : 'warning'}
                        size="small"
                      />
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
            )
          })}
        </Grid>
      ) : (
        <Card>
          <CardContent>
            <Typography align="center" color="textSecondary">
              No Ruuvi tags configured yet. Add your first tag to get started!
            </Typography>
          </CardContent>
        </Card>
      )}
    </Box>
  )
}

export default Dashboard