import {
  Card,
  CardBody,
  CardFooter,
  Divider,
  Image,
  Spacer,
} from '@nextui-org/react'
import { useEmployeeModel } from '../models/Employee'
import { useAuth0 } from '@auth0/auth0-react'
import '../index.css'
import LoadingComponent from '../components/loading'
export const ProfilePage = () => {
  const { currentEmployee, isLoading: isCurrentEmployeeLoading } =
    useEmployeeModel()
  const { user, isLoading } = useAuth0()
  if (isLoading || isCurrentEmployeeLoading) {
    return <LoadingComponent />
  }
  return (
    <>
      <Card
        className="border-none bg-primary/90 w-[98vw] m-auto mt-2"
        shadow="lg"
        isBlurred
      >
        <CardBody className="flex flex-wrap flex-row justify-between py-4 pl-8">
          <div className="flex flex-wrap flex-col">
            <h1 className="font-bold text-[36px] text-white leading-none">
              {currentEmployee?.name}
            </h1>
            <h1 className="text-[16px] text-white leading-5">
              {currentEmployee?.email}
            </h1>
            <Spacer className="h-6" />
            <h1 className="text-[24px] text-white">{currentEmployee?.title}</h1>
            <h1 className="text-[24px] text-white">
              {currentEmployee?.country}
            </h1>
            {currentEmployee?.managedBy && (
              <h1 className="text-[24px] text-white">
                Manager: {currentEmployee?.managedBy}
              </h1>
            )}
          </div>
          <Image
            isBlurred
            alt="Profile image"
            src={user?.picture}
            width={180}
            height={180}
          />
        </CardBody>
      </Card>
      <Card
        className="bg-default-500 w-[98vw] m-auto mt-2"
        shadow="lg"
        isBlurred
      >
        <CardBody className="flex flex-wrap flex-col py-4 pl-8">
          <h1 className="font-bold text-[24px] text-white ">
            You have {currentEmployee?.paidTimeOff} days of paid time off
            anually.
          </h1>

          <h1 className="text-[20px] text-white">
            - Days not used this year: {currentEmployee?.paidTimeOffLeft}
          </h1>

          <h1 className="text-[20px] text-white">
            - Days not used next year:{' '}
            {currentEmployee?.paidTimeOffLeftNextYear}
          </h1>
        </CardBody>
      </Card>
      {currentEmployee?.subordinates &&
        currentEmployee.subordinates.length > 0 && (
          <Card
            className=" bg-default-400 w-[98vw] m-auto mt-2"
            shadow="lg"
            isBlurred
          >
            <CardBody className="flex flex-wrap flex-col py-4 pl-8">
              <h1 className="font-bold text-[24px] text-white ">
                Your direct reports:
              </h1>
              {currentEmployee.subordinates.map((employee) => (
                <h1 className="text-[20px] text-white">- {employee.email}</h1>
              ))}
            </CardBody>
          </Card>
        )}
    </>
  )
}
