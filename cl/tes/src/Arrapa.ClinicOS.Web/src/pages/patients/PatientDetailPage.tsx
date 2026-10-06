import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate, useParams } from 'react-router-dom';

import { ArrowLeftIcon, PencilSquareIcon, TrashIcon, PlusIcon } from '@heroicons/react/24/outline';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Avatar,
  Badge,
  Button,
  ConfirmDialog,
  Drawer,
  FormField,
  Input,
  PageHeader,
  SkeletonCard,
  Spinner,
  Switch,
  Textarea,
} from '@/components/ui';
import {
  useDeleteAllergy,
  useDeleteMedicalHistory,
  useDeleteMedication,
  useDeletePatient,
  usePatient,
  usePatientAllergies,
  usePatientMedicalHistory,
  usePatientMedications,
  useSetPatientActive,
  useUpdatePatient,
  useUpsertAllergy,
  useUpsertMedicalHistory,
  useUpsertMedication,
} from '@/features/patients/hooks';
import {
  allergySchema,
  medicalHistorySchema,
  medicationSchema,
  patientSchema,
  type AllergyFormValues,
  type MedicalHistoryFormValues,
  type MedicationFormValues,
  type PatientFormValues,
} from '@/features/patients/schemas';
import {
  ALLERGY_SEVERITY_OPTIONS,
  BLOOD_GROUP_LABEL,
  type AllergySeverity,
  type MedicalHistoryEntry,
  type PatientAllergy,
  type PatientMedication,
} from '@/features/patients/types';

import { PatientForm } from './PatientForm';
import { toPatientPayload } from './toPatientPayload';

const severityTone: Record<AllergySeverity, 'gray' | 'amber' | 'red'> = {
  Unknown: 'gray',
  Mild: 'gray',
  Moderate: 'amber',
  Severe: 'red',
  LifeThreatening: 'red',
};

export default function PatientDetailPage() {
  const { id } = useParams<{ id: string }>();
  const patientId = id!;
  const navigate = useNavigate();

  const patient = usePatient(patientId);
  const allergies = usePatientAllergies(patientId);
  const history = usePatientMedicalHistory(patientId);
  const medications = usePatientMedications(patientId, true);
  const setActive = useSetPatientActive();
  const removePatient = useDeletePatient();

  const [editingPatient, setEditingPatient] = useState(false);
  const [editAllergy, setEditAllergy] = useState<PatientAllergy | 'new' | null>(null);
  const [editHistory, setEditHistory] = useState<MedicalHistoryEntry | 'new' | null>(null);
  const [editMedication, setEditMedication] = useState<PatientMedication | 'new' | null>(null);
  const [confirmDelete, setConfirmDelete] = useState(false);

  if (patient.isPending) {
    return (
      <div className="space-y-6">
        <PageHeader eyebrow="Patient" title="Loading patient…" />
        <SkeletonCard lines={4} />
      </div>
    );
  }
  if (patient.isError || !patient.data) {
    return (
      <div className="space-y-6">
        <PageHeader eyebrow="Patient" title="Not found" />
        <Alert variant="error">{humanizeError(patient.error)}</Alert>
      </div>
    );
  }

  const p = patient.data;

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow={
          <Link to="/patients" className="inline-flex items-center gap-1 hover:text-brand-700 dark:hover:text-brand-300">
            <ArrowLeftIcon className="h-3.5 w-3.5" /> Back to patients
          </Link>
        }
        title={p.fullName}
        description={
          <span className="inline-flex items-center gap-2 flex-wrap">
            <span className="text-caption font-mono text-gray-500">{p.mrn}</span>
            <span className="text-gray-400">·</span>
            <span className="text-body text-gray-600 dark:text-gray-300">{p.age} yrs · {p.gender}</span>
            {p.bloodGroup !== 'Unknown' && <Badge tone="red">{BLOOD_GROUP_LABEL[p.bloodGroup]}</Badge>}
            <Badge tone={p.isActive ? 'green' : 'gray'} variant="dot">{p.isActive ? 'Active' : 'Inactive'}</Badge>
          </span>
        }
        actions={
          <>
            <Button variant="secondary" onClick={() => setEditingPatient(true)} leftIcon={<PencilSquareIcon className="h-4 w-4" />}>
              Edit
            </Button>
            {p.isActive ? (
              <Button variant="danger" onClick={() => setActive.mutate({ id: p.id, isActive: false })}>
                Deactivate
              </Button>
            ) : (
              <Button onClick={() => setActive.mutate({ id: p.id, isActive: true })}>Activate</Button>
            )}
          </>
        }
      />

      {/* Patient hero */}
      <section className="rounded-xl border border-gray-200/70 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-theme-xs p-5 sm:p-6 flex items-start gap-4">
        <Avatar name={p.fullName} src={p.photoUrl} size="xl" />
        <div className="flex-1 min-w-0 grid grid-cols-2 sm:grid-cols-4 gap-4">
          <Meta label="Date of birth" value={p.dateOfBirth} />
          <Meta label="Marital status" value={p.maritalStatus} />
          <Meta label="Blood group" value={BLOOD_GROUP_LABEL[p.bloodGroup] ?? '—'} />
          <Meta label="Occupation" value={p.occupation ?? '—'} />
        </div>
      </section>

      <section className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card label="Contact">
          <Row label="Phone" value={p.phone} />
          <Row label="Email" value={p.email} />
          <Row label="Occupation" value={p.occupation} />
        </Card>
        <Card label="Address">
          <p className="text-sm text-gray-800 dark:text-gray-100 whitespace-pre-line">
            {[p.addressLine1, p.addressLine2, [p.city, p.state, p.postalCode].filter(Boolean).join(', '), p.country]
              .filter(Boolean)
              .join('\n') || <span className="text-gray-400">—</span>}
          </p>
        </Card>
        <Card label="Insurance">
          <Row label="Provider" value={p.insuranceProvider} />
          <Row label="Policy" value={p.insurancePolicyNumber} mono />
          <Row label="Valid until" value={p.insuranceValidUntil ?? undefined} />
        </Card>
      </section>

      <section className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <Card label="Emergency contact">
          <Row label="Name" value={p.emergencyContactName} />
          <Row label="Phone" value={p.emergencyContactPhone} />
          <Row label="Relationship" value={p.emergencyContactRelationship} />
        </Card>
        <Card label="Guardian">
          <Row label="Name" value={p.guardianName} />
          <Row label="Phone" value={p.guardianPhone} />
          <Row label="Relationship" value={p.guardianRelationship} />
        </Card>
      </section>

      {/* Allergies */}
      <SectionHeader
        title="Allergies"
        subtitle={`${allergies.data?.length ?? 0} recorded`}
        action={
          <Button size="sm" variant="secondary" onClick={() => setEditAllergy('new')} leftIcon={<PlusIcon className="h-3.5 w-3.5" />}>
            Add allergy
          </Button>
        }
      />
      <SectionBody>
        {allergies.isPending && <Loading />}
        {allergies.data && allergies.data.length === 0 && <EmptyRow>No known allergies.</EmptyRow>}
        {allergies.data && allergies.data.length > 0 && (
          <ul className="divide-y divide-gray-100 dark:divide-gray-800">
            {allergies.data.map((a) => (
              <li key={a.id} className="py-3 flex items-center justify-between gap-3">
                <div>
                  <p className="text-sm font-medium text-gray-900 dark:text-white">{a.name}</p>
                  {a.reactionDescription && <p className="text-xs text-gray-500 mt-0.5 line-clamp-2">{a.reactionDescription}</p>}
                </div>
                <div className="flex items-center gap-2 shrink-0">
                  <Badge tone={severityTone[a.severity]}>{a.severity}</Badge>
                  <Button size="sm" variant="ghost" onClick={() => setEditAllergy(a)}>Edit</Button>
                </div>
              </li>
            ))}
          </ul>
        )}
      </SectionBody>

      {/* Medical history */}
      <SectionHeader
        title="Medical history"
        subtitle={`${history.data?.length ?? 0} entries`}
        action={
          <Button size="sm" variant="secondary" onClick={() => setEditHistory('new')} leftIcon={<PlusIcon className="h-3.5 w-3.5" />}>
            Add entry
          </Button>
        }
      />
      <SectionBody>
        {history.isPending && <Loading />}
        {history.data && history.data.length === 0 && <EmptyRow>No medical history recorded.</EmptyRow>}
        {history.data && history.data.length > 0 && (
          <ul className="divide-y divide-gray-100 dark:divide-gray-800">
            {history.data.map((h) => (
              <li key={h.id} className="py-3 flex items-center justify-between gap-3">
                <div>
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-medium text-gray-900 dark:text-white">{h.condition}</p>
                    {h.isChronic && <Badge tone="amber">Chronic</Badge>}
                    {h.isResolved && <Badge tone="green">Resolved</Badge>}
                  </div>
                  <p className="text-xs text-gray-500 mt-0.5">
                    {h.since ? `Since ${h.since}` : 'Onset unknown'}
                    {h.notes ? ` · ${h.notes}` : ''}
                  </p>
                </div>
                <Button size="sm" variant="ghost" onClick={() => setEditHistory(h)}>Edit</Button>
              </li>
            ))}
          </ul>
        )}
      </SectionBody>

      {/* Medications */}
      <SectionHeader
        title="Current medications"
        subtitle={`${medications.data?.filter((m) => m.isActive).length ?? 0} active`}
        action={
          <Button size="sm" variant="secondary" onClick={() => setEditMedication('new')} leftIcon={<PlusIcon className="h-3.5 w-3.5" />}>
            Add medication
          </Button>
        }
      />
      <SectionBody>
        {medications.isPending && <Loading />}
        {medications.data && medications.data.length === 0 && <EmptyRow>No medications on file.</EmptyRow>}
        {medications.data && medications.data.length > 0 && (
          <ul className="divide-y divide-gray-100 dark:divide-gray-800">
            {medications.data.map((m) => (
              <li key={m.id} className="py-3 flex items-center justify-between gap-3">
                <div>
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-medium text-gray-900 dark:text-white">{m.name}</p>
                    {!m.isActive && <Badge tone="gray">Stopped</Badge>}
                  </div>
                  <p className="text-xs text-gray-500 mt-0.5">
                    {[m.dosage, m.frequency, m.startedOn ? `since ${m.startedOn}` : null].filter(Boolean).join(' · ')}
                    {m.notes ? ` · ${m.notes}` : ''}
                  </p>
                </div>
                <Button size="sm" variant="ghost" onClick={() => setEditMedication(m)}>Edit</Button>
              </li>
            ))}
          </ul>
        )}
      </SectionBody>

      {/* Notes */}
      {p.notes && (
        <>
          <SectionHeader title="Notes" />
          <SectionBody>
            <p className="text-sm text-gray-700 dark:text-gray-100 whitespace-pre-line">{p.notes}</p>
          </SectionBody>
        </>
      )}

      <div className="pt-2 pb-6">
        <Button variant="danger" onClick={() => setConfirmDelete(true)} leftIcon={<TrashIcon className="h-4 w-4" />}>
          Remove patient
        </Button>
      </div>

      {/* Edit patient drawer */}
      <EditPatientDrawer
        open={editingPatient}
        onClose={() => setEditingPatient(false)}
        patientId={patientId}
        initial={p}
      />

      {/* Allergy drawer */}
      {editAllergy && (
        <AllergyDrawer
          open={!!editAllergy}
          onClose={() => setEditAllergy(null)}
          patientId={patientId}
          allergy={editAllergy === 'new' ? undefined : editAllergy}
        />
      )}

      {/* Medical history drawer */}
      {editHistory && (
        <HistoryDrawer
          open={!!editHistory}
          onClose={() => setEditHistory(null)}
          patientId={patientId}
          entry={editHistory === 'new' ? undefined : editHistory}
        />
      )}

      {/* Medication drawer */}
      {editMedication && (
        <MedicationDrawer
          open={!!editMedication}
          onClose={() => setEditMedication(null)}
          patientId={patientId}
          medication={editMedication === 'new' ? undefined : editMedication}
        />
      )}

      <ConfirmDialog
        open={confirmDelete}
        onClose={() => setConfirmDelete(false)}
        onConfirm={() =>
          removePatient.mutate(patientId, {
            onSuccess: () => navigate('/patients', { replace: true }),
          })
        }
        title="Remove patient"
        description="This soft-deletes the patient record. Their history remains recoverable by a super admin."
        variant="danger"
        confirmLabel="Remove"
        isLoading={removePatient.isPending}
      />
    </div>
  );
}

// -------------------- Card / Row helpers --------------------
function Card({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="rounded-xl border border-gray-200/70 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-theme-xs p-5">
      <p className="u-label mb-3">{label}</p>
      <div className="space-y-3">{children}</div>
    </div>
  );
}

function Row({ label, value, mono }: { label: string; value?: string | null; mono?: boolean }) {
  return (
    <div>
      <p className="text-[10px] uppercase tracking-widest text-gray-400 font-medium">{label}</p>
      <p className={`text-body text-gray-900 dark:text-gray-100 ${mono ? 'font-mono' : ''}`}>
        {value || <span className="text-gray-400">—</span>}
      </p>
    </div>
  );
}

function Meta({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-[10px] uppercase tracking-widest text-gray-400 font-medium">{label}</p>
      <p className="text-body text-gray-900 dark:text-white font-medium mt-0.5">{value}</p>
    </div>
  );
}

function SectionHeader({ title, subtitle, action }: { title: string; subtitle?: string; action?: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between mt-2">
      <div>
        <h2 className="text-title text-gray-900 dark:text-white">{title}</h2>
        {subtitle && <p className="text-caption text-gray-500 dark:text-gray-400 mt-0.5">{subtitle}</p>}
      </div>
      {action}
    </div>
  );
}

function SectionBody({ children }: { children: React.ReactNode }) {
  return (
    <div className="rounded-xl border border-gray-200/70 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-theme-xs px-5 py-2">
      {children}
    </div>
  );
}

function Loading() {
  return (
    <div className="flex items-center justify-center py-6 text-gray-500 text-sm">
      <Spinner className="h-4 w-4 mr-2" /> Loading…
    </div>
  );
}

function EmptyRow({ children }: { children: React.ReactNode }) {
  return <p className="py-4 text-sm text-gray-500 text-center">{children}</p>;
}

// -------------------- Drawers --------------------
function EditPatientDrawer({
  open,
  onClose,
  patientId,
  initial,
}: {
  open: boolean;
  onClose: () => void;
  patientId: string;
  initial: import('@/features/patients/types').PatientDetail;
}) {
  const update = useUpdatePatient(patientId);

  const form = useForm<PatientFormValues>({
    resolver: zodResolver(patientSchema),
    defaultValues: {
      firstName: initial.firstName,
      middleName: initial.middleName ?? '',
      lastName: initial.lastName,
      dateOfBirth: initial.dateOfBirth,
      gender: initial.gender,
      bloodGroup: initial.bloodGroup,
      maritalStatus: initial.maritalStatus,
      phone: initial.phone ?? '',
      email: initial.email ?? '',
      photoUrl: initial.photoUrl ?? '',
      addressLine1: initial.addressLine1 ?? '',
      addressLine2: initial.addressLine2 ?? '',
      city: initial.city ?? '',
      state: initial.state ?? '',
      country: initial.country ?? '',
      postalCode: initial.postalCode ?? '',
      occupation: initial.occupation ?? '',
      emergencyContactName: initial.emergencyContactName ?? '',
      emergencyContactPhone: initial.emergencyContactPhone ?? '',
      emergencyContactRelationship: initial.emergencyContactRelationship ?? '',
      guardianName: initial.guardianName ?? '',
      guardianPhone: initial.guardianPhone ?? '',
      guardianRelationship: initial.guardianRelationship ?? '',
      insuranceProvider: initial.insuranceProvider ?? '',
      insurancePolicyNumber: initial.insurancePolicyNumber ?? '',
      insuranceGroupNumber: initial.insuranceGroupNumber ?? '',
      insuranceValidUntil: initial.insuranceValidUntil ?? '',
      notes: initial.notes ?? '',
    },
  });

  useEffect(() => {
    if (open) {
      form.reset({
        firstName: initial.firstName,
        middleName: initial.middleName ?? '',
        lastName: initial.lastName,
        dateOfBirth: initial.dateOfBirth,
        gender: initial.gender,
        bloodGroup: initial.bloodGroup,
        maritalStatus: initial.maritalStatus,
        phone: initial.phone ?? '',
        email: initial.email ?? '',
        photoUrl: initial.photoUrl ?? '',
        addressLine1: initial.addressLine1 ?? '',
        addressLine2: initial.addressLine2 ?? '',
        city: initial.city ?? '',
        state: initial.state ?? '',
        country: initial.country ?? '',
        postalCode: initial.postalCode ?? '',
        occupation: initial.occupation ?? '',
        emergencyContactName: initial.emergencyContactName ?? '',
        emergencyContactPhone: initial.emergencyContactPhone ?? '',
        emergencyContactRelationship: initial.emergencyContactRelationship ?? '',
        guardianName: initial.guardianName ?? '',
        guardianPhone: initial.guardianPhone ?? '',
        guardianRelationship: initial.guardianRelationship ?? '',
        insuranceProvider: initial.insuranceProvider ?? '',
        insurancePolicyNumber: initial.insurancePolicyNumber ?? '',
        insuranceGroupNumber: initial.insuranceGroupNumber ?? '',
        insuranceValidUntil: initial.insuranceValidUntil ?? '',
        notes: initial.notes ?? '',
      });
    }
  }, [open, initial, form]);

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await update.mutateAsync(toPatientPayload(values));
      onClose();
    } catch { /* shown */ }
  });

  return (
    <Drawer
      open={open}
      onClose={onClose}
      title={`Edit ${initial.fullName}`}
      size="lg"
      footer={
        <div className="flex items-center justify-end gap-3">
          <Button variant="secondary" onClick={onClose}>Cancel</Button>
          <Button onClick={onSubmit} isLoading={update.isPending}>Save changes</Button>
        </div>
      }
    >
      {update.isError && <Alert variant="error" className="mb-4">{humanizeError(update.error)}</Alert>}
      <form onSubmit={onSubmit} noValidate>
        <PatientForm form={form} />
      </form>
    </Drawer>
  );
}

function AllergyDrawer({
  open,
  onClose,
  patientId,
  allergy,
}: {
  open: boolean;
  onClose: () => void;
  patientId: string;
  allergy?: PatientAllergy;
}) {
  const upsert = useUpsertAllergy(patientId, allergy?.id);
  const remove = useDeleteAllergy(patientId);

  const form = useForm<AllergyFormValues>({
    resolver: zodResolver(allergySchema),
    defaultValues: {
      name: allergy?.name ?? '',
      severity: allergy?.severity ?? 'Unknown',
      reactionDescription: allergy?.reactionDescription ?? '',
      identifiedOn: allergy?.identifiedOn ?? '',
    },
  });

  useEffect(() => {
    form.reset({
      name: allergy?.name ?? '',
      severity: allergy?.severity ?? 'Unknown',
      reactionDescription: allergy?.reactionDescription ?? '',
      identifiedOn: allergy?.identifiedOn ?? '',
    });
  }, [allergy, form]);

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await upsert.mutateAsync({
        name: values.name.trim(),
        severity: values.severity,
        reactionDescription: values.reactionDescription?.trim() || null,
        identifiedOn: values.identifiedOn || null,
      });
      onClose();
    } catch { /* shown */ }
  });

  return (
    <Drawer
      open={open}
      onClose={onClose}
      title={allergy ? 'Edit allergy' : 'Add allergy'}
      footer={
        <div className="flex items-center justify-between gap-3">
          {allergy && (
            <Button
              variant="danger"
              onClick={() => remove.mutate(allergy.id, { onSuccess: onClose })}
              isLoading={remove.isPending}
            >
              Delete
            </Button>
          )}
          <div className="flex-1" />
          <Button variant="secondary" onClick={onClose}>Cancel</Button>
          <Button onClick={onSubmit} isLoading={upsert.isPending}>{allergy ? 'Save' : 'Add'}</Button>
        </div>
      }
    >
      {upsert.isError && <Alert variant="error" className="mb-4">{humanizeError(upsert.error)}</Alert>}
      <form onSubmit={onSubmit} className="space-y-4" noValidate>
        <FormField id="name" label="Allergen" required error={form.formState.errors.name?.message}>
          <Input id="name" {...form.register('name')} />
        </FormField>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="severity" label="Severity" required>
            <select
              id="severity"
              {...form.register('severity')}
              className="h-11 w-full rounded-lg border bg-white pl-3.5 pr-9 text-sm text-gray-900 shadow-theme-xs focus:border-brand-500 focus:outline-none focus:ring-2 focus:ring-brand-500/25 dark:bg-gray-900 dark:text-gray-100 dark:border-gray-700 border-gray-200"
            >
              {ALLERGY_SEVERITY_OPTIONS.map((s) => (
                <option key={s} value={s}>{s}</option>
              ))}
            </select>
          </FormField>
          <FormField id="identifiedOn" label="Identified on">
            <Input id="identifiedOn" type="date" {...form.register('identifiedOn')} />
          </FormField>
        </div>
        <FormField id="reactionDescription" label="Reaction description" error={form.formState.errors.reactionDescription?.message}>
          <Textarea id="reactionDescription" rows={3} {...form.register('reactionDescription')} />
        </FormField>
      </form>
    </Drawer>
  );
}

function HistoryDrawer({
  open,
  onClose,
  patientId,
  entry,
}: {
  open: boolean;
  onClose: () => void;
  patientId: string;
  entry?: MedicalHistoryEntry;
}) {
  const upsert = useUpsertMedicalHistory(patientId, entry?.id);
  const remove = useDeleteMedicalHistory(patientId);

  const form = useForm<MedicalHistoryFormValues>({
    resolver: zodResolver(medicalHistorySchema),
    defaultValues: {
      condition: entry?.condition ?? '',
      since: entry?.since ?? '',
      notes: entry?.notes ?? '',
      isChronic: entry?.isChronic ?? false,
      isResolved: entry?.isResolved ?? false,
    },
  });

  useEffect(() => {
    form.reset({
      condition: entry?.condition ?? '',
      since: entry?.since ?? '',
      notes: entry?.notes ?? '',
      isChronic: entry?.isChronic ?? false,
      isResolved: entry?.isResolved ?? false,
    });
  }, [entry, form]);

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await upsert.mutateAsync({
        condition: values.condition.trim(),
        since: values.since || null,
        notes: values.notes?.trim() || null,
        isChronic: values.isChronic,
        isResolved: values.isResolved,
      });
      onClose();
    } catch { /* shown */ }
  });

  return (
    <Drawer
      open={open}
      onClose={onClose}
      title={entry ? 'Edit history entry' : 'Add history entry'}
      footer={
        <div className="flex items-center justify-between gap-3">
          {entry && (
            <Button
              variant="danger"
              onClick={() => remove.mutate(entry.id, { onSuccess: onClose })}
              isLoading={remove.isPending}
            >
              Delete
            </Button>
          )}
          <div className="flex-1" />
          <Button variant="secondary" onClick={onClose}>Cancel</Button>
          <Button onClick={onSubmit} isLoading={upsert.isPending}>{entry ? 'Save' : 'Add'}</Button>
        </div>
      }
    >
      {upsert.isError && <Alert variant="error" className="mb-4">{humanizeError(upsert.error)}</Alert>}
      <form onSubmit={onSubmit} className="space-y-4" noValidate>
        <FormField id="condition" label="Condition" required error={form.formState.errors.condition?.message}>
          <Input id="condition" {...form.register('condition')} />
        </FormField>
        <FormField id="since" label="Since">
          <Input id="since" type="date" {...form.register('since')} />
        </FormField>
        <FormField id="notes" label="Notes" error={form.formState.errors.notes?.message}>
          <Textarea id="notes" rows={3} {...form.register('notes')} />
        </FormField>
        <div className="flex items-center gap-6">
          <Switch
            checked={form.watch('isChronic')}
            onCheckedChange={(v) => form.setValue('isChronic', v, { shouldDirty: true })}
            label="Chronic"
          />
          <Switch
            checked={form.watch('isResolved')}
            onCheckedChange={(v) => form.setValue('isResolved', v, { shouldDirty: true })}
            label="Resolved"
          />
        </div>
      </form>
    </Drawer>
  );
}

function MedicationDrawer({
  open,
  onClose,
  patientId,
  medication,
}: {
  open: boolean;
  onClose: () => void;
  patientId: string;
  medication?: PatientMedication;
}) {
  const upsert = useUpsertMedication(patientId, medication?.id);
  const remove = useDeleteMedication(patientId);

  const form = useForm<MedicationFormValues>({
    resolver: zodResolver(medicationSchema),
    defaultValues: {
      name: medication?.name ?? '',
      dosage: medication?.dosage ?? '',
      frequency: medication?.frequency ?? '',
      startedOn: medication?.startedOn ?? '',
      notes: medication?.notes ?? '',
      isActive: medication?.isActive ?? true,
    },
  });

  useEffect(() => {
    form.reset({
      name: medication?.name ?? '',
      dosage: medication?.dosage ?? '',
      frequency: medication?.frequency ?? '',
      startedOn: medication?.startedOn ?? '',
      notes: medication?.notes ?? '',
      isActive: medication?.isActive ?? true,
    });
  }, [medication, form]);

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await upsert.mutateAsync({
        name: values.name.trim(),
        dosage: values.dosage?.trim() || null,
        frequency: values.frequency?.trim() || null,
        startedOn: values.startedOn || null,
        notes: values.notes?.trim() || null,
        isActive: values.isActive,
      });
      onClose();
    } catch { /* shown */ }
  });

  return (
    <Drawer
      open={open}
      onClose={onClose}
      title={medication ? 'Edit medication' : 'Add medication'}
      footer={
        <div className="flex items-center justify-between gap-3">
          {medication && (
            <Button
              variant="danger"
              onClick={() => remove.mutate(medication.id, { onSuccess: onClose })}
              isLoading={remove.isPending}
            >
              Delete
            </Button>
          )}
          <div className="flex-1" />
          <Button variant="secondary" onClick={onClose}>Cancel</Button>
          <Button onClick={onSubmit} isLoading={upsert.isPending}>{medication ? 'Save' : 'Add'}</Button>
        </div>
      }
    >
      {upsert.isError && <Alert variant="error" className="mb-4">{humanizeError(upsert.error)}</Alert>}
      <form onSubmit={onSubmit} className="space-y-4" noValidate>
        <FormField id="name" label="Medication" required error={form.formState.errors.name?.message}>
          <Input id="name" {...form.register('name')} />
        </FormField>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="dosage" label="Dosage">
            <Input id="dosage" placeholder="e.g. 500 mg" {...form.register('dosage')} />
          </FormField>
          <FormField id="frequency" label="Frequency">
            <Input id="frequency" placeholder="e.g. Twice daily" {...form.register('frequency')} />
          </FormField>
        </div>
        <FormField id="startedOn" label="Started on">
          <Input id="startedOn" type="date" {...form.register('startedOn')} />
        </FormField>
        <FormField id="notes" label="Notes" error={form.formState.errors.notes?.message}>
          <Textarea id="notes" rows={3} {...form.register('notes')} />
        </FormField>
        <Switch
          checked={form.watch('isActive')}
          onCheckedChange={(v) => form.setValue('isActive', v, { shouldDirty: true })}
          label="Actively taking"
        />
      </form>
    </Drawer>
  );
}
