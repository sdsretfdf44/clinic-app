import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  ConfirmDialog,
  DataTable,
  Drawer,
  FormField,
  Input,
  PageHeader,
  Pagination,
  Select,
  Switch,
  Textarea,
  type Column,
} from '@/components/ui';
import { useDepartments } from '@/features/clinic/departments';
import {
  createDoctorSchema,
  updateDoctorSchema,
  useCreateDoctor,
  useDeleteDoctor,
  useDoctor,
  useDoctorsList,
  useSetDoctorActive,
  useUpdateDoctor,
  type CreateDoctorFormValues,
  type DoctorSummary,
  type UpdateDoctorFormValues,
} from '@/features/clinic/doctors';
import { useSetDoctorHours, useDoctorHours, type WorkingHoursSlot } from '@/features/clinic/workingHours';

const DAY_NAMES = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];

export default function DoctorsPage() {
  const [pageNumber, setPageNumber] = useState(1);
  const [search, setSearch] = useState('');
  const [departmentId, setDepartmentId] = useState('');
  const [activeFilter, setActiveFilter] = useState<'all' | 'active' | 'inactive'>('all');
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [mode, setMode] = useState<'create' | 'edit'>('create');
  const [editingId, setEditingId] = useState<string | null>(null);
  const [confirmDelete, setConfirmDelete] = useState<DoctorSummary | null>(null);

  const departments = useDepartments(true);
  const params = {
    pageNumber,
    pageSize: 20,
    search: search || undefined,
    departmentId: departmentId || undefined,
    isActive: activeFilter === 'all' ? undefined : activeFilter === 'active',
  };
  const doctorsList = useDoctorsList(params);
  const createDoctor = useCreateDoctor();
  const setDoctorActive = useSetDoctorActive();
  const deleteDoctor = useDeleteDoctor();

  const columns: Column<DoctorSummary>[] = [
    {
      key: 'name',
      header: 'Doctor',
      render: (d) => (
        <div>
          <p className="font-medium text-gray-900 dark:text-white">{d.fullName}</p>
          <p className="text-xs text-gray-500">{d.email}</p>
        </div>
      ),
    },
    { key: 'department', header: 'Department', render: (d) => d.departmentName ?? <span className="text-gray-400">—</span> },
    { key: 'specialty', header: 'Specialty', render: (d) => d.specialty ?? <span className="text-gray-400">—</span> },
    {
      key: 'fee',
      header: 'Fee',
      render: (d) => (d.effectiveConsultationFee != null ? `${d.effectiveConsultationFee.toFixed(2)} ${d.currency}` : '—'),
    },
    {
      key: 'status',
      header: 'Status',
      render: (d) => (
        <div className="flex items-center gap-2">
          <Badge tone={d.isActive ? 'green' : 'gray'}>{d.isActive ? 'Active' : 'Inactive'}</Badge>
          {d.isAcceptingNewPatients && <Badge tone="brand">New patients</Badge>}
        </div>
      ),
    },
    {
      key: 'actions',
      header: '',
      align: 'right',
      render: (d) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="secondary"
            onClick={(e) => {
              e.stopPropagation();
              setMode('edit');
              setEditingId(d.id);
              setDrawerOpen(true);
            }}
          >
            Edit
          </Button>
          <Button
            size="sm"
            variant={d.isActive ? 'danger' : 'primary'}
            onClick={(e) => {
              e.stopPropagation();
              setDoctorActive.mutate({ id: d.id, isActive: !d.isActive });
            }}
          >
            {d.isActive ? 'Deactivate' : 'Activate'}
          </Button>
          <Button
            size="sm"
            variant="ghost"
            onClick={(e) => {
              e.stopPropagation();
              setConfirmDelete(d);
            }}
          >
            Remove
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Clinic admin"
        title="Doctors"
        description="Provision doctors, assign departments, set consultation fees and working hours."
        actions={
          <Button
            onClick={() => {
              setMode('create');
              setEditingId(null);
              setDrawerOpen(true);
            }}
          >
            New doctor
          </Button>
        }
      />

      <div className="flex flex-col sm:flex-row gap-3">
        <Input
          placeholder="Search by name, email, specialty"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPageNumber(1);
          }}
        />
        <Select
          className="sm:max-w-xs"
          value={departmentId}
          onChange={(e) => {
            setDepartmentId(e.target.value);
            setPageNumber(1);
          }}
        >
          <option value="">All departments</option>
          {departments.data?.map((d) => (
            <option key={d.id} value={d.id}>{d.name}</option>
          ))}
        </Select>
        <Select
          className="sm:max-w-xs"
          value={activeFilter}
          onChange={(e) => {
            setActiveFilter(e.target.value as typeof activeFilter);
            setPageNumber(1);
          }}
        >
          <option value="all">All</option>
          <option value="active">Active</option>
          <option value="inactive">Inactive</option>
        </Select>
      </div>

      {doctorsList.isError && <Alert variant="error">{humanizeError(doctorsList.error)}</Alert>}

      <DataTable
        columns={columns}
        rows={doctorsList.data?.items ?? []}
        keyExtractor={(d) => d.id}
        isLoading={doctorsList.isPending}
      />

      {doctorsList.data && (
        <Pagination
          pageNumber={doctorsList.data.pageNumber}
          pageSize={doctorsList.data.pageSize}
          totalCount={doctorsList.data.totalCount}
          totalPages={doctorsList.data.totalPages}
          onPageChange={setPageNumber}
        />
      )}

      {mode === 'create' && (
        <CreateDoctorDrawer
          open={drawerOpen}
          onClose={() => setDrawerOpen(false)}
          departments={departments.data ?? []}
          createDoctor={createDoctor}
        />
      )}

      {mode === 'edit' && editingId && (
        <EditDoctorDrawer
          open={drawerOpen}
          onClose={() => setDrawerOpen(false)}
          doctorId={editingId}
          departments={departments.data ?? []}
        />
      )}

      <ConfirmDialog
        open={!!confirmDelete}
        onClose={() => setConfirmDelete(null)}
        onConfirm={() => {
          if (!confirmDelete) return;
          deleteDoctor.mutate(confirmDelete.id, { onSettled: () => setConfirmDelete(null) });
        }}
        title="Remove doctor"
        description={confirmDelete ? `Remove ${confirmDelete.fullName}? Their user account will be deactivated.` : null}
        variant="danger"
        confirmLabel="Remove"
        isLoading={deleteDoctor.isPending}
      />
    </div>
  );
}

function CreateDoctorDrawer({
  open,
  onClose,
  departments,
  createDoctor,
}: {
  open: boolean;
  onClose: () => void;
  departments: { id: string; name: string }[];
  createDoctor: ReturnType<typeof useCreateDoctor>;
}) {
  const form = useForm<CreateDoctorFormValues>({
    resolver: zodResolver(createDoctorSchema),
    defaultValues: {
      firstName: '', lastName: '', email: '', phoneNumber: '',
      specialty: '', licenseNumber: '', registrationNumber: '',
      departmentId: '', consultationFee: '', bio: '',
      isAcceptingNewPatients: true,
    },
  });

  useEffect(() => {
    if (open) form.reset();
  }, [open, form]);

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await createDoctor.mutateAsync({
        firstName: values.firstName.trim(),
        lastName: values.lastName.trim(),
        email: values.email.trim(),
        phoneNumber: values.phoneNumber?.trim() || null,
        specialty: values.specialty?.trim() || null,
        licenseNumber: values.licenseNumber?.trim() || null,
        registrationNumber: values.registrationNumber?.trim() || null,
        departmentId: values.departmentId || null,
        consultationFee:
          values.consultationFee === '' || values.consultationFee === undefined
            ? null
            : Number(values.consultationFee),
        bio: values.bio?.trim() || null,
        isAcceptingNewPatients: values.isAcceptingNewPatients,
      });
      onClose();
    } catch {
      /* shown via mutation state */
    }
  });

  return (
    <Drawer
      open={open}
      onClose={onClose}
      title="New doctor"
      description="A user account will be provisioned with a temporary password sent via email."
      size="lg"
      footer={
        <div className="flex items-center justify-end gap-3">
          <Button variant="secondary" onClick={onClose}>Cancel</Button>
          <Button onClick={onSubmit} isLoading={createDoctor.isPending}>Create doctor</Button>
        </div>
      }
    >
      {createDoctor.isError && <Alert variant="error" className="mb-4">{humanizeError(createDoctor.error)}</Alert>}
      <form onSubmit={onSubmit} className="space-y-4" noValidate>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="firstName" label="First name" required error={form.formState.errors.firstName?.message}>
            <Input id="firstName" invalid={!!form.formState.errors.firstName} {...form.register('firstName')} />
          </FormField>
          <FormField id="lastName" label="Last name" required error={form.formState.errors.lastName?.message}>
            <Input id="lastName" invalid={!!form.formState.errors.lastName} {...form.register('lastName')} />
          </FormField>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="email" label="Email" required error={form.formState.errors.email?.message}>
            <Input id="email" type="email" invalid={!!form.formState.errors.email} {...form.register('email')} />
          </FormField>
          <FormField id="phoneNumber" label="Phone">
            <Input id="phoneNumber" type="tel" {...form.register('phoneNumber')} />
          </FormField>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="specialty" label="Specialty">
            <Input id="specialty" {...form.register('specialty')} />
          </FormField>
          <FormField id="departmentId" label="Department">
            <Select id="departmentId" {...form.register('departmentId')}>
              <option value="">— none —</option>
              {departments.map((d) => (
                <option key={d.id} value={d.id}>{d.name}</option>
              ))}
            </Select>
          </FormField>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <FormField id="licenseNumber" label="License number">
            <Input id="licenseNumber" {...form.register('licenseNumber')} />
          </FormField>
          <FormField id="registrationNumber" label="Registration number">
            <Input id="registrationNumber" {...form.register('registrationNumber')} />
          </FormField>
        </div>
        <FormField id="consultationFee" label="Consultation fee" hint="Overrides the department/clinic default.">
          <Input id="consultationFee" type="number" step="0.01" min="0" {...form.register('consultationFee')} />
        </FormField>
        <FormField id="bio" label="Bio">
          <Textarea id="bio" rows={3} {...form.register('bio')} />
        </FormField>
        <Switch
          checked={form.watch('isAcceptingNewPatients')}
          onCheckedChange={(v) => form.setValue('isAcceptingNewPatients', v, { shouldDirty: true })}
          label="Accepting new patients"
        />
      </form>
    </Drawer>
  );
}

function EditDoctorDrawer({
  open,
  onClose,
  doctorId,
  departments,
}: {
  open: boolean;
  onClose: () => void;
  doctorId: string;
  departments: { id: string; name: string }[];
}) {
  const doctor = useDoctor(open ? doctorId : undefined);
  const updateDoctor = useUpdateDoctor(doctorId);
  const doctorHours = useDoctorHours(open ? doctorId : undefined);
  const setDoctorHours = useSetDoctorHours(doctorId);

  const form = useForm<UpdateDoctorFormValues>({
    resolver: zodResolver(updateDoctorSchema),
    defaultValues: {
      firstName: '', lastName: '', phoneNumber: '', avatarUrl: '',
      specialty: '', licenseNumber: '', registrationNumber: '',
      departmentId: '', consultationFee: '', bio: '',
      isAcceptingNewPatients: true, sortOrder: 0,
    },
  });

  const [slots, setSlots] = useState<WorkingHoursSlot[]>([]);

  useEffect(() => {
    if (doctor.data) {
      form.reset({
        firstName: doctor.data.firstName,
        lastName: doctor.data.lastName,
        phoneNumber: doctor.data.phoneNumber ?? '',
        avatarUrl: doctor.data.avatarUrl ?? '',
        specialty: doctor.data.specialty ?? '',
        licenseNumber: doctor.data.licenseNumber ?? '',
        registrationNumber: doctor.data.registrationNumber ?? '',
        departmentId: doctor.data.departmentId ?? '',
        consultationFee: doctor.data.consultationFee ?? '',
        bio: doctor.data.bio ?? '',
        isAcceptingNewPatients: doctor.data.isAcceptingNewPatients,
        sortOrder: doctor.data.sortOrder,
      });
    }
  }, [doctor.data, form]);

  useEffect(() => {
    if (doctorHours.data) setSlots(doctorHours.data.slots);
  }, [doctorHours.data]);

  const onSubmit = form.handleSubmit(async (values) => {
    try {
      await updateDoctor.mutateAsync({
        firstName: values.firstName.trim(),
        lastName: values.lastName.trim(),
        phoneNumber: values.phoneNumber?.trim() || null,
        avatarUrl: values.avatarUrl?.trim() || null,
        specialty: values.specialty?.trim() || null,
        licenseNumber: values.licenseNumber?.trim() || null,
        registrationNumber: values.registrationNumber?.trim() || null,
        departmentId: values.departmentId || null,
        consultationFee:
          values.consultationFee === '' || values.consultationFee === undefined
            ? null
            : Number(values.consultationFee),
        bio: values.bio?.trim() || null,
        isAcceptingNewPatients: values.isAcceptingNewPatients,
        sortOrder: Number(values.sortOrder),
      });
      await setDoctorHours.mutateAsync(slots);
      onClose();
    } catch {
      /* shown */
    }
  });

  const addSlot = (day: number) => setSlots((prev) => [...prev, { dayOfWeek: day, startTime: '09:00', endTime: '17:00' }]);
  const removeSlot = (index: number) => setSlots((prev) => prev.filter((_, i) => i !== index));
  const updateSlot = (index: number, patch: Partial<WorkingHoursSlot>) =>
    setSlots((prev) => prev.map((s, i) => (i === index ? { ...s, ...patch } : s)));

  return (
    <Drawer
      open={open}
      onClose={onClose}
      title={doctor.data?.fullName ?? 'Doctor'}
      size="lg"
      footer={
        <div className="flex items-center justify-end gap-3">
          <Button variant="secondary" onClick={onClose}>Cancel</Button>
          <Button onClick={onSubmit} isLoading={updateDoctor.isPending || setDoctorHours.isPending}>Save changes</Button>
        </div>
      }
    >
      {(updateDoctor.isError || setDoctorHours.isError) && (
        <Alert variant="error" className="mb-4">{humanizeError(updateDoctor.error ?? setDoctorHours.error)}</Alert>
      )}
      <form onSubmit={onSubmit} className="space-y-6" noValidate>
        <section>
          <h4 className="text-sm font-semibold text-gray-900 dark:text-white mb-3">Profile</h4>
          <div className="space-y-4">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <FormField id="firstName" label="First name" required error={form.formState.errors.firstName?.message}>
                <Input id="firstName" {...form.register('firstName')} />
              </FormField>
              <FormField id="lastName" label="Last name" required error={form.formState.errors.lastName?.message}>
                <Input id="lastName" {...form.register('lastName')} />
              </FormField>
              <FormField id="phoneNumber" label="Phone">
                <Input id="phoneNumber" type="tel" {...form.register('phoneNumber')} />
              </FormField>
              <FormField id="avatarUrl" label="Avatar URL" error={form.formState.errors.avatarUrl?.message}>
                <Input id="avatarUrl" type="url" {...form.register('avatarUrl')} />
              </FormField>
              <FormField id="specialty" label="Specialty">
                <Input id="specialty" {...form.register('specialty')} />
              </FormField>
              <FormField id="departmentId" label="Department">
                <Select id="departmentId" {...form.register('departmentId')}>
                  <option value="">— none —</option>
                  {departments.map((d) => (
                    <option key={d.id} value={d.id}>{d.name}</option>
                  ))}
                </Select>
              </FormField>
              <FormField id="licenseNumber" label="License #">
                <Input id="licenseNumber" {...form.register('licenseNumber')} />
              </FormField>
              <FormField id="registrationNumber" label="Registration #">
                <Input id="registrationNumber" {...form.register('registrationNumber')} />
              </FormField>
              <FormField id="consultationFee" label="Consultation fee">
                <Input id="consultationFee" type="number" step="0.01" min="0" {...form.register('consultationFee')} />
              </FormField>
              <FormField id="sortOrder" label="Sort order">
                <Input id="sortOrder" type="number" min="0" {...form.register('sortOrder')} />
              </FormField>
            </div>
            <FormField id="bio" label="Bio">
              <Textarea id="bio" rows={3} {...form.register('bio')} />
            </FormField>
            <Switch
              checked={form.watch('isAcceptingNewPatients')}
              onCheckedChange={(v) => form.setValue('isAcceptingNewPatients', v, { shouldDirty: true })}
              label="Accepting new patients"
            />
          </div>
        </section>

        <section>
          <h4 className="text-sm font-semibold text-gray-900 dark:text-white mb-3">Working hours</h4>
          <div className="space-y-3">
            {DAY_NAMES.map((day, dayIndex) => {
              const daySlots = slots.map((s, i) => ({ ...s, i })).filter((s) => s.dayOfWeek === dayIndex);
              return (
                <div key={day} className="rounded-lg border border-gray-100 dark:border-gray-800 p-3">
                  <div className="flex items-center justify-between mb-2">
                    <span className="text-sm font-medium text-gray-800 dark:text-gray-200">{day}</span>
                    <Button type="button" size="sm" variant="ghost" onClick={() => addSlot(dayIndex)}>+ Add slot</Button>
                  </div>
                  {daySlots.length === 0 ? (
                    <p className="text-xs text-gray-500">Off</p>
                  ) : (
                    <div className="space-y-2">
                      {daySlots.map((slot) => (
                        <div key={slot.i} className="flex items-center gap-2">
                          <Input
                            type="time"
                            value={slot.startTime}
                            onChange={(e) => updateSlot(slot.i, { startTime: e.target.value })}
                            className="max-w-[8rem]"
                          />
                          <span className="text-xs text-gray-500">→</span>
                          <Input
                            type="time"
                            value={slot.endTime}
                            onChange={(e) => updateSlot(slot.i, { endTime: e.target.value })}
                            className="max-w-[8rem]"
                          />
                          <Button type="button" size="sm" variant="ghost" onClick={() => removeSlot(slot.i)}>
                            Remove
                          </Button>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </section>
      </form>
    </Drawer>
  );
}
