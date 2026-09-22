import { useLocation } from 'react-router-dom'

export function LocationProbe() {
  return <div data-testid="location">{useLocation().pathname}</div>
}
