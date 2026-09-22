import type { MouseEvent } from 'react'
import { Link, type LinkProps } from 'react-router-dom'
import { useNavigationGuard } from '../lib/navigationGuard'

type Props = Omit<LinkProps, 'to'> & { to: string }

// An in-app link that an edited form can intercept (WI-004 DEC-022, DD-003-SPD §9). A modified click (new tab or
// window) is never intercepted: it leaves the current page and its edits untouched.
export function GuardedLink({ to, onClick, ...rest }: Props) {
  const guard = useNavigationGuard()

  function handleClick(event: MouseEvent<HTMLAnchorElement>) {
    onClick?.(event)
    if (event.defaultPrevented) {
      return
    }
    const modified = event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey
    if (!modified && guard.intercepts(to)) {
      event.preventDefault()
    }
  }

  return <Link to={to} onClick={handleClick} {...rest} />
}
