import { Routes, Route } from 'react-router-dom'
import { Box } from '@mui/material'
import Layout from './components/Layout'
import Dashboard from './pages/Dashboard'
import TagList from './pages/TagList'
import TagDetails from './pages/TagDetails'
import AddTag from './pages/AddTag'
import EditTag from './pages/EditTag'

function App() {
  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <Layout>
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/tags" element={<TagList />} />
          <Route path="/tags/new" element={<AddTag />} />
          <Route path="/tags/:macAddress" element={<TagDetails />} />
          <Route path="/tags/:macAddress/edit" element={<EditTag />} />
        </Routes>
      </Layout>
    </Box>
  )
}

export default App