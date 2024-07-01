import { useAuth0 } from '@auth0/auth0-react'

export const ProfilePage = () => {
  const { user } = useAuth0()

  if (!user) {
    return null
  }

  return (
    <>
      <h1>Profile Page</h1>
      <div>
        <img src={user.picture} alt="Profile" />
        <div>
          <h2>{user.name}</h2>
          <p>{user.email}</p>
        </div>
      </div>
    </>
  )
}
