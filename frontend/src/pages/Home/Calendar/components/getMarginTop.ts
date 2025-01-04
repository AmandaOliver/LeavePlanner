import { DateTime, Interval } from 'luxon'
import { LeaveType } from '../../../../models/Leaves'

const leavesOverlaps = (
  leave1: LeaveType,
  leaves: LeaveType[],
  processedLeaves: LeaveType[]
) => {
  const leave1Interval = Interval.fromDateTimes(
    DateTime.fromISO(leave1.dateStart),
    DateTime.fromISO(leave1.dateEnd)
  )
  const leaveThatOverlaps = leaves
    .sort((a, b) => (a.dateStart < b.dateStart ? 1 : -1))
    .filter(
      (l) =>
        l.id !== leave1.id &&
        !processedLeaves.includes(l) &&
        DateTime.fromISO(l.dateStart) <= DateTime.fromISO(leave1.dateStart)
    )
    .filter((l) => {
      const interval = Interval.fromDateTimes(
        DateTime.fromISO(l.dateStart),
        DateTime.fromISO(l.dateEnd)
      )
      return interval.overlaps(leave1Interval)
    })
  return leaveThatOverlaps
}
export const getSpaces = (
  leave: LeaveType,
  leaves: LeaveType[],
  dayDate: DateTime,
  processedLeaves: LeaveType[]
) => {
  //   if (leave.id == '1581' && dayDate.day == 31) debugger

  let spaces = 0
  const previousLeaves = leavesOverlaps(leave, leaves, processedLeaves)
  if (previousLeaves.length > 0) {
    const leaveInterval = Interval.fromDateTimes(
      DateTime.fromISO(previousLeaves[0].dateStart),
      DateTime.fromISO(previousLeaves[0].dateEnd)
    )
    if (!leaveInterval.contains(dayDate)) {
      spaces += 1
      processedLeaves.push(leave)

      spaces += getSpaces(previousLeaves[0], leaves, dayDate, processedLeaves)
    }
  }

  return spaces
}
