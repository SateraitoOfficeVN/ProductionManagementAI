import { Route, Routes } from 'react-router-dom'
import { NavigationGuardProvider } from './components/NavigationGuardProvider'
import { LoginPage } from './features/auth/LoginPage'
import { ProtectedRoute } from './features/auth/ProtectedRoute'
import { DashboardPage } from './features/dashboard/DashboardPage'
import { ProductionOrderListPage } from './features/production-orders/ProductionOrderListPage'
import { ProductionOrderPage } from './features/production-orders/ProductionOrderPage'

function App() {
  // The navigation guard lets an edited Screen A form intercept in-app links (WI-004 DEC-022).
  return (
    <NavigationGuardProvider>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <DashboardPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/production-orders"
          element={
            <ProtectedRoute>
              <ProductionOrderListPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/production-orders/new"
          element={
            <ProtectedRoute>
              <ProductionOrderPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/production-orders/:id"
          element={
            <ProtectedRoute>
              <ProductionOrderPage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </NavigationGuardProvider>
  )
}

export default App
