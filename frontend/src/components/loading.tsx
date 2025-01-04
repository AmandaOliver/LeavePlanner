import { CircularProgress } from '@nextui-org/react'

export const LoadingComponent = () => (
  <div className="flex flex-wrap justify-center h-screen align-middle">
    <CircularProgress size={'lg'} aria-label="Loading..." />
  </div>
)
