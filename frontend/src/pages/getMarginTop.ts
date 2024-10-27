import { DateTime, Interval } from 'luxon'
import { LeaveType } from '../models/Leaves'

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
    .sort((leaveA, leaveB) => {
      const startA = DateTime.fromISO(leaveA.dateStart)
      const startB = DateTime.fromISO(leaveB.dateStart)

      // First, compare by start date
      const startComparison = startA < startB ? -1 : startA > startB ? 1 : 0

      if (startComparison !== 0) {
        return startComparison
      }

      // If start dates are the same, compare by duration (end date - start date)
      const durationA = DateTime.fromISO(leaveA.dateEnd).diff(
        startA,
        'days'
      ).days
      const durationB = DateTime.fromISO(leaveB.dateEnd).diff(
        startB,
        'days'
      ).days

      return durationA - durationB
    })
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
