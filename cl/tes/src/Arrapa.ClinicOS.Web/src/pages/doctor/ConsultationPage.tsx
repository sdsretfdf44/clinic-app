import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useFieldArray, useForm } from 'react-hook-form';
import { Link, useNavigate, useParams } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  ConfirmDialog,
  FormField,
  Input,
  PageHeader,
  Spinner,
  Textarea,
} from '@/components/ui';
import { useAppointment, useTransitionAppointment } from '@/features/appointments';
import { usePatient, usePatientAllergies, usePatientMedications } from '@/features/patients/hooks';
import {
  diagnosisSchema,
  prescriptionSchema,
  useAddPrescription,
  useDeletePrescription,
  useUpdatePrescription,
  useUpsertDiagnosis,
  useUpsertVitals,
  useVisitByAppointment,
  vitalsSchema,
  type DiagnosisFormValues,
  type Prescription,
  type PrescriptionFormValues,
  type VisitDetail,
  type VitalsFormValues,
} from '@/features/visits';

import { STATUS_LABEL, STATUS_TONE } from '../appointments/statusMeta';

function nullifyEmptyNumber(v: number | '' | undefined): number | null {
  return v === '' || v === undefined || Number.isNaN(Number(v)) ? null : Number(v);
}

function nullifyEmptyString(v: string | undefined): string | null {
  const t = v?.trim();
  return !t ? null : t;
}

export default function ConsultationPage() {
  const { appointmentId } = useParams<{ appointmentId: string }>();
  const navigate = useNavigate();

  const appointment = useAppointment(appointmentId);
  const visit = useVisitByAppointment(appointmentId);
  const transition = useTransitionAppointment();

  const [completeOpen, setCompleteOpen] = useState(false);

  if (appointment.isPending || visit.isPending) {
    return (
      <div className="flex items-center justify-center py-24 text-gray-500">
        <Spinner className="h-6 w-6 mr-2" /> Loading consultation…
      </div>
    );
  }
  if (appointment.isError || !appointment.data) {
    return <Alert variant="error">{humanizeError(appointment.error)}</Alert>;
  }
  if (visit.isError || !visit.data) {
    return (
      <div className="space-y-4">
        <Alert variant="warning" title="No visit yet">
          A visit is created automatically when the appointment enters consultation. Move the appointment to
          <em> In consultation</em> first.
        </Alert>
        <Button variant="secondary" onClick={() => navigate(`/appointments/${appointmentId}`)}>
          Back to appointment
        </Button>
      </div>
    );
  }

  const a = appointment.data;
  const v = visit.data;
  const canEdit = !v.isLocked;

  return (
    <div className="space-y-6 max-w-5xl">
      <PageHeader
        eyebrow={<Link to="/doctor/queue" className="hover:text-brand-600">← Back to queue</Link>}
        title={`Consultation — ${v.patientName}`}
        description={
          <span className="flex items-center gap-2 flex-wrap">
            <span className="font-mono text-xs">{a.appointmentNumber}</span>
            <span>·</span>
            <span>{v.patientAge} yrs · {v.doctorName}</span>
            <Badge tone={STATUS_TONE[a.status]}>{STATUS_LABEL[a.status]}</Badge>
            {v.isLocked && <Badge tone="gray">Locked</Badge>}
          </span>
        }
        actions={
          <>
            {a.status === 'InConsultation' && (
              <Button variant="primary" onClick={() => setCompleteOpen(true)}>Complete visit</Button>
            )}
            <Button variant="secondary" onClick={() => navigate(`/patients/${v.patientId}`)}>Open chart</Button>
          </>
        }
      />

      {/* Patient safety strip */}
      <PatientSafetyStrip patientId={v.patientId} />

      {/* Vitals */}
      <VitalsSection visit={v} canEdit={canEdit} />

      {/* Diagnosis / notes */}
      <DiagnosisSection visit={v} canEdit={canEdit} />

      {/* Prescriptions */}
      <PrescriptionsSection visit={v} canEdit={canEdit} />

      <ConfirmDialog
        open={completeOpen}
        onClose={() => setCompleteOpen(false)}
        onConfirm={() =>
          transition.mutate(
            { id: a.id, action: 'complete' },
            {
              onSuccess: () => {
                setCompleteOpen(false);
                appointment.refetch();
                visit.refetch();
              },
            },
          )
        }
        title="Complete visit"
        description="This locks the visit for further edits. Vitals, diagnosis, and prescriptions become read-only."
        confirmLabel="Complete"
        isLoading={transition.isPending}
      />
    </div>
  );
}

// -------------------- Patient safety strip --------------------
function PatientSafetyStrip({ patientId }: { patientId: string }) {
  const patient = usePatient(patientId);
  const allergies = usePatientAllergies(patientId);
  const medications = usePatientMedications(patientId, false);

  const criticalAllergies = allergies.data?.filter((a) => a.severity === 'Severe' || a.severity === 'LifeThreatening');

  return (
    <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5">
      <p className="text-xs uppercase tracking-widest text-gray-400 mb-3">Patient safety</p>
      {patient.data?.bloodGroup && patient.data.bloodGroup !== 'Unknown' && (
        <p className="text-sm text-gray-800 dark:text-gray-100 mb-2">
          Blood group: <Badge tone="red">{patient.data.bloodGroup}</Badge>
        </p>
      )}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-2">
        <div>
          <p className="text-xs uppercase text-gray-400 mb-1">Allergies</p>
          {allergies.data && allergies.data.length === 0 && (
            <p className="text-sm text-gray-500">No known allergies.</p>
          )}
          {criticalAllergies && criticalAllergies.length > 0 && (
            <Alert variant="error" title="Critical allergies">
              {criticalAllergies.map((a) => (
                <p key={a.id}><strong>{a.name}</strong>{a.reactionDescription ? ` — ${a.reactionDescription}` : ''}</p>
              ))}
            </Alert>
          )}
          {allergies.data && allergies.data.length > 0 && !criticalAllergies?.length && (
            <ul className="text-sm text-gray-700 dark:text-gray-200 space-y-1">
              {allergies.data.map((a) => (
                <li key={a.id}>
                  {a.name} <span className="text-xs text-gray-500">({a.severity})</span>
                </li>
              ))}
            </ul>
          )}
        </div>
        <div>
          <p className="text-xs uppercase text-gray-400 mb-1">Current medications</p>
          {medications.data && medications.data.length === 0 && (
            <p className="text-sm text-gray-500">None on file.</p>
          )}
          {medications.data && medications.data.length > 0 && (
            <ul className="text-sm text-gray-700 dark:text-gray-200 space-y-1">
              {medications.data.map((m) => (
                <li key={m.id}>
                  {m.name}
                  {m.dosage ? <span className="text-xs text-gray-500"> {m.dosage}</span> : null}
                  {m.frequency ? <span className="text-xs text-gray-500"> · {m.frequency}</span> : null}
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </section>
  );
}

// -------------------- Vitals --------------------
function VitalsSection({ visit, canEdit }: { visit: VisitDetail; canEdit: boolean }) {
  const upsert = useUpsertVitals(visit.id);

  const form = useForm<VitalsFormValues>({
    resolver: zodResolver(vitalsSchema),
    defaultValues: {
      bloodPressureSystolic: visit.vitals?.bloodPressureSystolic ?? '',
      bloodPressureDiastolic: visit.vitals?.bloodPressureDiastolic ?? '',
      pulseBpm: visit.vitals?.pulseBpm ?? '',
      temperatureCelsius: visit.vitals?.temperatureCelsius ?? '',
      heightCm: visit.vitals?.heightCm ?? '',
      weightKg: visit.vitals?.weightKg ?? '',
      spO2Percent: visit.vitals?.spO2Percent ?? '',
      respirationRate: visit.vitals?.respirationRate ?? '',
      notes: visit.vitals?.notes ?? '',
    },
  });

  useEffect(() => {
    form.reset({
      bloodPressureSystolic: visit.vitals?.bloodPressureSystolic ?? '',
      bloodPressureDiastolic: visit.vitals?.bloodPressureDiastolic ?? '',
      pulseBpm: visit.vitals?.pulseBpm ?? '',
      temperatureCelsius: visit.vitals?.temperatureCelsius ?? '',
      heightCm: visit.vitals?.heightCm ?? '',
      weightKg: visit.vitals?.weightKg ?? '',
      spO2Percent: visit.vitals?.spO2Percent ?? '',
      respirationRate: visit.vitals?.respirationRate ?? '',
      notes: visit.vitals?.notes ?? '',
    });
  }, [visit.vitals, form]);

  const onSubmit = form.handleSubmit((values) => {
    upsert.mutate({
      bloodPressureSystolic: nullifyEmptyNumber(values.bloodPressureSystolic as number | '' | undefined),
      bloodPressureDiastolic: nullifyEmptyNumber(values.bloodPressureDiastolic as number | '' | undefined),
      pulseBpm: nullifyEmptyNumber(values.pulseBpm as number | '' | undefined),
      temperatureCelsius: nullifyEmptyNumber(values.temperatureCelsius as number | '' | undefined),
      heightCm: nullifyEmptyNumber(values.heightCm as number | '' | undefined),
      weightKg: nullifyEmptyNumber(values.weightKg as number | '' | undefined),
      spO2Percent: nullifyEmptyNumber(values.spO2Percent as number | '' | undefined),
      respirationRate: nullifyEmptyNumber(values.respirationRate as number | '' | undefined),
      notes: nullifyEmptyString(values.notes),
    });
  });

  return (
    <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-sm font-semibold text-gray-900 dark:text-white">Vitals</h3>
        {visit.vitals?.bmi != null && <Badge tone="brand">BMI {visit.vitals.bmi}</Badge>}
      </div>

      {upsert.isError && <Alert variant="error" className="mb-3">{humanizeError(upsert.error)}</Alert>}

      <form onSubmit={onSubmit} className="space-y-4">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <FormField id="bpSys" label="BP systolic (mmHg)">
            <Input id="bpSys" type="number" disabled={!canEdit} {...form.register('bloodPressureSystolic')} />
          </FormField>
          <FormField id="bpDia" label="BP diastolic (mmHg)">
            <Input id="bpDia" type="number" disabled={!canEdit} {...form.register('bloodPressureDiastolic')} />
          </FormField>
          <FormField id="pulse" label="Pulse (bpm)">
            <Input id="pulse" type="number" disabled={!canEdit} {...form.register('pulseBpm')} />
          </FormField>
          <FormField id="temp" label="Temperature (°C)">
            <Input id="temp" type="number" step="0.1" disabled={!canEdit} {...form.register('temperatureCelsius')} />
          </FormField>
          <FormField id="height" label="Height (cm)">
            <Input id="height" type="number" step="0.1" disabled={!canEdit} {...form.register('heightCm')} />
          </FormField>
          <FormField id="weight" label="Weight (kg)">
            <Input id="weight" type="number" step="0.1" disabled={!canEdit} {...form.register('weightKg')} />
          </FormField>
          <FormField id="spo2" label="SpO₂ (%)">
            <Input id="spo2" type="number" disabled={!canEdit} {...form.register('spO2Percent')} />
          </FormField>
          <FormField id="rr" label="Respiration (/min)">
            <Input id="rr" type="number" disabled={!canEdit} {...form.register('respirationRate')} />
          </FormField>
        </div>
        <FormField id="vitalsNotes" label="Notes">
          <Textarea id="vitalsNotes" rows={2} disabled={!canEdit} {...form.register('notes')} />
        </FormField>
        {canEdit && (
          <div className="flex justify-end">
            <Button type="submit" isLoading={upsert.isPending}>Save vitals</Button>
          </div>
        )}
      </form>
    </section>
  );
}

// -------------------- Diagnosis --------------------
function DiagnosisSection({ visit, canEdit }: { visit: VisitDetail; canEdit: boolean }) {
  const upsert = useUpsertDiagnosis(visit.id);

  const form = useForm<DiagnosisFormValues>({
    resolver: zodResolver(diagnosisSchema),
    defaultValues: {
      symptoms: visit.symptoms ?? '',
      diagnosis: visit.diagnosis ?? '',
      doctorNotes: visit.doctorNotes ?? '',
    },
  });

  useEffect(() => {
    form.reset({
      symptoms: visit.symptoms ?? '',
      diagnosis: visit.diagnosis ?? '',
      doctorNotes: visit.doctorNotes ?? '',
    });
  }, [visit.symptoms, visit.diagnosis, visit.doctorNotes, form]);

  const onSubmit = form.handleSubmit((values) => {
    upsert.mutate({
      symptoms: nullifyEmptyString(values.symptoms),
      diagnosis: nullifyEmptyString(values.diagnosis),
      doctorNotes: nullifyEmptyString(values.doctorNotes),
    });
  });

  return (
    <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5">
      <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-4">Symptoms · Diagnosis · Notes</h3>
      {upsert.isError && <Alert variant="error" className="mb-3">{humanizeError(upsert.error)}</Alert>}
      <form onSubmit={onSubmit} className="space-y-4">
        <FormField id="symptoms" label="Symptoms">
          <Textarea id="symptoms" rows={3} disabled={!canEdit} {...form.register('symptoms')} />
        </FormField>
        <FormField id="diagnosis" label="Diagnosis">
          <Textarea id="diagnosis" rows={3} disabled={!canEdit} {...form.register('diagnosis')} />
        </FormField>
        <FormField id="doctorNotes" label="Doctor's notes">
          <Textarea id="doctorNotes" rows={4} disabled={!canEdit} {...form.register('doctorNotes')} />
        </FormField>
        {canEdit && (
          <div className="flex justify-end">
            <Button type="submit" isLoading={upsert.isPending}>Save notes</Button>
          </div>
        )}
      </form>
    </section>
  );
}

// -------------------- Prescriptions --------------------
function PrescriptionsSection({ visit, canEdit }: { visit: VisitDetail; canEdit: boolean }) {
  const [editingId, setEditingId] = useState<string | 'new' | null>(null);
  const deletePrescription = useDeletePrescription(visit.id);

  return (
    <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-sm font-semibold text-gray-900 dark:text-white">Prescriptions</h3>
        {canEdit && (
          <Button size="sm" variant="secondary" onClick={() => setEditingId('new')}>+ New prescription</Button>
        )}
      </div>

      {visit.prescriptions.length === 0 && editingId !== 'new' && (
        <p className="text-sm text-gray-500">No prescriptions issued yet.</p>
      )}

      <div className="space-y-3">
        {visit.prescriptions.map((p) => (
          <div key={p.id} className="rounded-lg border border-gray-100 dark:border-gray-800 p-4">
            {editingId === p.id ? (
              <PrescriptionForm
                visitId={visit.id}
                initial={p}
                onDone={() => setEditingId(null)}
              />
            ) : (
              <>
                <div className="flex items-center justify-between mb-2">
                  <p className="text-xs text-gray-500">Issued {new Date(p.issuedOn).toLocaleString()}</p>
                  {canEdit && (
                    <div className="flex items-center gap-2">
                      <Button size="sm" variant="ghost" onClick={() => setEditingId(p.id)}>Edit</Button>
                      <Button
                        size="sm"
                        variant="danger"
                        onClick={() => deletePrescription.mutate(p.id)}
                        isLoading={deletePrescription.isPending}
                      >
                        Delete
                      </Button>
                    </div>
                  )}
                </div>
                {p.generalInstructions && (
                  <p className="text-sm text-gray-700 dark:text-gray-200 mb-2">{p.generalInstructions}</p>
                )}
                <ul className="space-y-1.5">
                  {p.items.map((i) => (
                    <li key={i.id} className="text-sm text-gray-800 dark:text-gray-100">
                      <strong>{i.medicine}</strong>
                      {i.strength ? <span className="text-gray-500"> {i.strength}</span> : null}
                      {i.dosage ? <span className="text-gray-500"> · {i.dosage}</span> : null}
                      {i.frequency ? <span className="text-gray-500"> · {i.frequency}</span> : null}
                      {i.duration ? <span className="text-gray-500"> · {i.duration}</span> : null}
                      {i.instructions ? <p className="text-xs text-gray-500 mt-0.5">{i.instructions}</p> : null}
                    </li>
                  ))}
                </ul>
              </>
            )}
          </div>
        ))}

        {editingId === 'new' && (
          <div className="rounded-lg border border-gray-100 dark:border-gray-800 p-4">
            <PrescriptionForm visitId={visit.id} onDone={() => setEditingId(null)} />
          </div>
        )}
      </div>
    </section>
  );
}

function PrescriptionForm({
  visitId,
  initial,
  onDone,
}: {
  visitId: string;
  initial?: Prescription;
  onDone: () => void;
}) {
  const add = useAddPrescription(visitId);
  const update = useUpdatePrescription(visitId, initial?.id ?? '');

  const form = useForm<PrescriptionFormValues>({
    resolver: zodResolver(prescriptionSchema),
    defaultValues: {
      generalInstructions: initial?.generalInstructions ?? '',
      items: initial?.items.length
        ? initial.items.map((i) => ({
            medicine: i.medicine,
            strength: i.strength ?? '',
            dosage: i.dosage ?? '',
            frequency: i.frequency ?? '',
            duration: i.duration ?? '',
            instructions: i.instructions ?? '',
          }))
        : [{ medicine: '', strength: '', dosage: '', frequency: '', duration: '', instructions: '' }],
    },
  });

  const items = useFieldArray({ control: form.control, name: 'items' });

  const onSubmit = form.handleSubmit(async (values) => {
    const payload = {
      generalInstructions: nullifyEmptyString(values.generalInstructions),
      items: values.items.map((i, index) => ({
        medicine: i.medicine.trim(),
        strength: nullifyEmptyString(i.strength),
        dosage: nullifyEmptyString(i.dosage),
        frequency: nullifyEmptyString(i.frequency),
        duration: nullifyEmptyString(i.duration),
        instructions: nullifyEmptyString(i.instructions),
        sortOrder: index,
      })),
    };
    try {
      if (initial) {
        await update.mutateAsync(payload);
      } else {
        await add.mutateAsync(payload);
      }
      onDone();
    } catch { /* shown */ }
  });

  return (
    <form onSubmit={onSubmit} className="space-y-4">
      {(add.isError || update.isError) && (
        <Alert variant="error">{humanizeError(add.error ?? update.error)}</Alert>
      )}

      <FormField id="genInstr" label="General instructions">
        <Textarea id="genInstr" rows={2} {...form.register('generalInstructions')} />
      </FormField>

      <div className="space-y-3">
        {items.fields.map((f, idx) => (
          <div key={f.id} className="rounded-lg border border-gray-100 dark:border-gray-800 p-3 space-y-3">
            <div className="flex items-center justify-between">
              <p className="text-xs uppercase tracking-widest text-gray-400">Medication {idx + 1}</p>
              {items.fields.length > 1 && (
                <Button type="button" size="sm" variant="ghost" onClick={() => items.remove(idx)}>Remove</Button>
              )}
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <FormField id={`medicine-${idx}`} label="Medicine" required error={form.formState.errors.items?.[idx]?.medicine?.message}>
                <Input {...form.register(`items.${idx}.medicine` as const)} />
              </FormField>
              <FormField id={`strength-${idx}`} label="Strength">
                <Input placeholder="500 mg" {...form.register(`items.${idx}.strength` as const)} />
              </FormField>
              <FormField id={`dosage-${idx}`} label="Dosage">
                <Input placeholder="1 tablet" {...form.register(`items.${idx}.dosage` as const)} />
              </FormField>
              <FormField id={`frequency-${idx}`} label="Frequency">
                <Input placeholder="Twice daily" {...form.register(`items.${idx}.frequency` as const)} />
              </FormField>
              <FormField id={`duration-${idx}`} label="Duration">
                <Input placeholder="7 days" {...form.register(`items.${idx}.duration` as const)} />
              </FormField>
              <FormField id={`instructions-${idx}`} label="Instructions">
                <Input placeholder="After meals" {...form.register(`items.${idx}.instructions` as const)} />
              </FormField>
            </div>
          </div>
        ))}
      </div>

      <div className="flex items-center justify-between">
        <Button
          type="button"
          variant="ghost"
          size="sm"
          onClick={() => items.append({ medicine: '', strength: '', dosage: '', frequency: '', duration: '', instructions: '' })}
        >
          + Add medication
        </Button>
        <div className="flex items-center gap-2">
          <Button type="button" variant="secondary" onClick={onDone}>Cancel</Button>
          <Button type="submit" isLoading={add.isPending || update.isPending}>
            {initial ? 'Save' : 'Add prescription'}
          </Button>
        </div>
      </div>
    </form>
  );
}
