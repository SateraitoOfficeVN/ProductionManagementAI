import type { DetailedHTMLProps, HTMLAttributes } from 'react'

// WI-014 (DEC-003): <selectedcontent> belongs to the HTML customizable select and is not yet in React's JSX types.
declare module 'react' {
  // eslint-disable-next-line @typescript-eslint/no-namespace
  namespace JSX {
    interface IntrinsicElements {
      selectedcontent: DetailedHTMLProps<HTMLAttributes<HTMLElement>, HTMLElement>
    }
  }
}
