import { useEffect, useState } from 'react';

import { humanizeError } from '@/api/httpClient';
import { Alert, Button, Input, PageHeader, Spinner } from '@/components/ui';
import { useClinicHours, useSetClinicHours, type WorkingHoursSlot } from '@/features/clinic/workingHours';

const DAYS = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

export default function WorkingHoursPage() {
  const hours = useClinicHours();
  const saveHours = useSetClinicHours();

  const [slots, setSlots] = useState<WorkingHoursSlot[]>([]);

  useEffect(() => {
    if (hours.data) setSlots(hours.data.slots);
  }, [hours.data]);

  const add = (day: number) => setSlots((s) => [...s, { dayOfWeek: day, startTime: '09:00', endTime: '17:00' }]);
  const remove = (i: number) => setSlots((s) => s.filter((_, idx) => idx !== i));
  const patch = (i: number, p: Partial<WorkingHoursSlot>) => setSlots((s) => s.map((v, idx) => (idx === i ? { ...v, ...p } : v)));

  if (hours.isPending) {
    return (
      <div className="flex items-center justify-center py-24 text-gray-500">
        <Spinner className="h-6 w-6 mr-2" /> Loading working hours…
      </div>
    );
  }

  return (
    <div className="space-y-4 max-w-3xl">
      <PageHeader
        eyebrow="Clinic admin"
        title="Clinic working hours"
        description="Default hours the clinic operates. Doctors can override these on their own profile."
        actions={<Button onClick={() => saveHours.mutate(slots)} isLoading={saveHours.isPending}>Save</Button>}
      />

      {hours.isError && <Alert variant="error">{humanizeError(hours.error)}</Alert>}
      {saveHours.isError && <Alert variant="error">{humanizeError(saveHours.error)}</Alert>}
      {saveHours.isSuccess && <Alert variant="success">Working hours saved.</Alert>}

      <div className="space-y-3">
        {DAYS.map((day, dayIndex) => {
          const daySlots = slots.map((s, i) => ({ ...s, i })).filter((s) => s.dayOfWeek === dayIndex);
          return (
            <div key={day} className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5">
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm font-medium text-gray-800 dark:text-gray-200">{day}</span>
                <Button type="button" size="sm" variant="ghost" onClick={() => add(dayIndex)}>+ Add slot</Button>
              </div>
              {daySlots.length === 0 ? (
                <p className="text-xs text-gray-500">Closed</p>
              ) : (
                <div className="space-y-2">
                  {daySlots.map((slot) => (
                    <div key={slot.i} className="flex items-center gap-2">
                      <Input
                        type="time"
                        value={slot.startTime}
                        onChange={(e) => patch(slot.i, { startTime: e.target.value })}
                        className="max-w-[8rem]"
                      />
                      <span className="text-xs text-gray-500">→</span>
                      <Input
                        type="time"
                        value={slot.endTime}
                        onChange={(e) => patch(slot.i, { endTime: e.target.value })}
                        className="max-w-[8rem]"
                      />
                      <Button type="button" size="sm" variant="ghost" onClick={() => remove(slot.i)}>Remove</Button>
                    </div>
                  ))}
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
}
