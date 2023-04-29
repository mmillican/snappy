import { DateTime } from 'luxon';

export function formatDate(value: string | null | undefined): string | null {
  if (!value) {
    return null;
  }

  const dt = DateTime.fromISO(value).toLocal();

  return dt.toFormat('yyyy-MM-dd');
}

export function friendlyLocalDate(value: string | null | undefined): string | null {
  if (!value) {
    return null;
  }

  const dt = DateTime.fromISO(value).toLocal();

  // return dt.toFormat('EEE, MMM d, yyyy, h:mm a');
  return dt.toLocaleString(DateTime.DATE_MED_WITH_WEEKDAY);
}

export function friendlyLocalDateTime(value: string | null | undefined): string | null {
  if (!value) {
    return null;
  }

  const dt = DateTime.fromISO(value).toLocal();

  // return dt.toFormat('EEE, MMM d, yyyy, h:mm a');
  return dt.toLocaleString(DateTime.DATETIME_MED_WITH_WEEKDAY);
}

export function relativeLocalTime(value: string | null | undefined): string | null {
  if (!value) {
    return null;
  }

  const dt = DateTime.fromISO(value).toLocal();
  return dt.toRelative();
}
