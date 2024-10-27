import { DateTime, Interval } from 'luxon'
import { LeaveType } from '../models/Leaves'

const leavesOverlaps = (leave1: LeaveType, leaves: LeaveType[]) => {
  const leave1Interval = Interval.fromDateTimes(
    DateTime.fromISO(leave1.dateStart),
    DateTime.fromISO(leave1.dateEnd)
  )
  const leaveThatOverlaps = leaves
    .sort((a, b) => (a.dateStart < b.dateStart ? 1 : -1))
    .filter(
      (l) =>
        l.id !== leave1.id &&
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
  dayDate: DateTime
) => {
  //   if (leave.id == '1574' && dayDate.day == 31) debugger

  let spaces = 0
  const previousLeaves = leavesOverlaps(leave, leaves)
  if (previousLeaves.length > 0) {
    const leave = previousLeaves[0]
    const leaveInterval = Interval.fromDateTimes(
      DateTime.fromISO(leave.dateStart),
      DateTime.fromISO(leave.dateEnd)
    )
    if (!leaveInterval.contains(dayDate)) {
      spaces += 1
      spaces += getSpaces(leave, leaves, dayDate)
    }
  }

  return spaces
}
