import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box,
  Button,
  Card,
  IconButton,
  Typography,
  Chip,
  CircularProgress,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
} from '@mui/material'
import { DataGrid, GridColDef, GridRenderCellParams } from '@mui/x-data-grid'
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as ViewIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material'
import { format, differenceInMinutes } from 'date-fns'
import { toast } from 'react-toastify'
import { ruuviTagsApi } from '../api/ruuviTags'
import { RuuviTagInfo } from '../types'

function TagList() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [deleteDialog, setDeleteDialog] = useState<{ open: boolean; tag: RuuviTagInfo | null }>({
    open: false,
    tag: null,
  })

  const { data: tags, isLoading, error, refetch } = useQuery({
    queryKey: ['ruuviTags'],
    queryFn: ruuviTagsApi.getAll,
  })

  const deleteMutation = useMutation({
    mutationFn: ruuviTagsApi.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ruuviTags'] })
      toast.success('Tag deleted successfully')
      setDeleteDialog({ open: false, tag: null })
    },
    onError: () => {
      toast.error('Failed to delete tag')
    },
  })

  const columns: GridColDef[] = [
    {
      field: 'status',
      headerName: 'Status',
      width: 100,
      renderCell: (params: GridRenderCellParams) => {
        const lastSeen = params.row.lastSeen
        const isActive = lastSeen && differenceInMinutes(new Date(), new Date(lastSeen)) < 5
        return (
          <Chip
            label={isActive ? 'Online' : 'Offline'}
            color={isActive ? 'success' : 'warning'}
            size="small"
          />
        )
      },
    },
    {
      field: 'name',
      headerName: 'Name',
      flex: 1,
      minWidth: 150,
    },
    {
      field: 'macAddress',
      headerName: 'MAC Address',
      width: 180,
      renderCell: (params: GridRenderCellParams) => (
        <Typography sx={{ fontFamily: 'monospace' }}>{params.value}</Typography>
      ),
    },
    {
      field: 'lastSeen',
      headerName: 'Last Seen',
      width: 200,
      renderCell: (params: GridRenderCellParams) => {
        if (!params.value) return <Typography color="textSecondary">Never</Typography>
        const minutesAgo = differenceInMinutes(new Date(), new Date(params.value))
        if (minutesAgo < 1) return 'Just now'
        if (minutesAgo < 60) return `${minutesAgo}m ago`
        if (minutesAgo < 1440) return `${Math.floor(minutesAgo / 60)}h ago`
        return format(new Date(params.value), 'MMM dd, HH:mm')
      },
    },
    {
      field: 'modificationTime',
      headerName: 'Modified',
      width: 180,
      renderCell: (params: GridRenderCellParams) => (
        <Typography variant="body2">
          {format(new Date(params.value), 'MMM dd, HH:mm')}
        </Typography>
      ),
    },
    {
      field: 'actions',
      headerName: 'Actions',
      width: 180,
      sortable: false,
      renderCell: (params: GridRenderCellParams) => (
        <Box>
          <IconButton
            size="small"
            onClick={() => navigate(`/tags/${params.row.macAddress}`)}
            title="View Details"
          >
            <ViewIcon />
          </IconButton>
          <IconButton
            size="small"
            onClick={() => navigate(`/tags/${params.row.macAddress}/edit`)}
            title="Edit"
          >
            <EditIcon />
          </IconButton>
          <IconButton
            size="small"
            onClick={() => setDeleteDialog({ open: true, tag: params.row })}
            title="Delete"
          >
            <DeleteIcon />
          </IconButton>
        </Box>
      ),
    },
  ]

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

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Ruuvi Tags</Typography>
        <Box display="flex" gap={2}>
          <Button
            startIcon={<RefreshIcon />}
            onClick={() => refetch()}
            variant="outlined"
          >
            Refresh
          </Button>
          <Button
            startIcon={<AddIcon />}
            onClick={() => navigate('/tags/new')}
            variant="contained"
          >
            Add Tag
          </Button>
        </Box>
      </Box>

      {tags && tags.length > 0 ? (
        <Card>
          <DataGrid
            rows={tags}
            columns={columns}
            getRowId={(row) => row.macAddress}
            initialState={{
              pagination: {
                paginationModel: { pageSize: 10 },
              },
            }}
            pageSizeOptions={[5, 10, 25, 50]}
            disableRowSelectionOnClick
            autoHeight
            sx={{
              '& .MuiDataGrid-cell': {
                borderBottom: 'none',
              },
              '& .MuiDataGrid-columnHeaders': {
                backgroundColor: 'background.default',
                borderBottom: 2,
                borderColor: 'divider',
              },
            }}
          />
        </Card>
      ) : (
        <Card>
          <Box p={4} textAlign="center">
            <Typography variant="h6" gutterBottom>
              No Ruuvi Tags Found
            </Typography>
            <Typography color="textSecondary" paragraph>
              You haven't added any Ruuvi tags yet. Click the button below to add your first tag.
            </Typography>
            <Button
              startIcon={<AddIcon />}
              onClick={() => navigate('/tags/new')}
              variant="contained"
              size="large"
            >
              Add Your First Tag
            </Button>
          </Box>
        </Card>
      )}

      <Dialog
        open={deleteDialog.open}
        onClose={() => setDeleteDialog({ open: false, tag: null })}
      >
        <DialogTitle>Delete Ruuvi Tag</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete the tag "{deleteDialog.tag?.name}"
            ({deleteDialog.tag?.macAddress})? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialog({ open: false, tag: null })}>
            Cancel
          </Button>
          <Button
            onClick={() => deleteDialog.tag && deleteMutation.mutate(deleteDialog.tag.macAddress)}
            color="error"
            variant="contained"
          >
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default TagList