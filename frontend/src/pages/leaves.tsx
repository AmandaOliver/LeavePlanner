import { useLeavesModel } from '../models/Leaves'

export const Leaves = ({ employeeEmail }: { employeeEmail: string }) => {
  const { leaves } = useLeavesModel(employeeEmail)
  return (
    <ul>
      {leaves.map((leave) => (
        <li key={leave.id}>
          {leave.type} {leave.dateStart}-{leave.dateEnd}: {leave.description}
        </li>
      ))}
    </ul>
  )
}
