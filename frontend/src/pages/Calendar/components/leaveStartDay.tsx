import { Tooltip } from '@nextui-org/react'
import { Interval, DateTime } from 'luxon'
import { ReactNode } from 'react'
import { BussinessWatchIcon } from '../../../icons/bussinesswatch'
import { PartyIcon } from '../../../icons/party'
import { LeaveType } from '../../../models/Leaves'

export const LeaveStartDay = ({
  leave,
  dayDate,
  background,
  common,
  distanceTop,
}: {
  leave: LeaveType
  dayDate: DateTime
  background: string
  common: string
  distanceTop: string
}) => {
  const leaveDuration = Interval.fromDateTimes(
    DateTime.fromISO(leave.dateStart),
    DateTime.fromISO(leave.dateEnd)
  )
    .toDuration('days')
    ?.toObject().days

  const text = 'ml-1 text-white'
  const name = <p className="pl-2 inline">{leave.ownerName}</p>
  const textConfig =
    leaveDuration && leaveDuration < 2
      ? 'rounded-lg truncate overflow-hidden'
      : 'rounded-l-lg whitespace-nowrap overflow-visible z-10'
  const TooltipElement = ({ children }: { children: ReactNode }) => (
    <Tooltip
      content={
        leave.type === 'bankHoliday'
          ? 'Bank Holiday'
          : leave.approvedBy
            ? 'Paid Time Off'
            : 'Request'
      }
    >
      {children}
    </Tooltip>
  )
  return (
    <TooltipElement>
      <div
        key={`leave-${dayDate}`}
        className={`${background} ${common} ${text} ${textConfig}`}
        style={{ marginTop: distanceTop }}
      >
        {name}
      </div>
    </TooltipElement>
  )
}
