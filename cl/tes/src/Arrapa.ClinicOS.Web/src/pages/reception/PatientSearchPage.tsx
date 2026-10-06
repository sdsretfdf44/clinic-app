import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  DataTable,
  EmptyState,
  Input,
  PageHeader,
  type Column,
} from '@/components/ui';
import { usePatientsList } from '@/features/patients/hooks';
import { BLOOD_GROUP_LABEL, type PatientSummary } from '@/features/patients/types';

/**
 * Scanner-friendly patient search. Autofocuses the input, submits on Enter, and gives one-click quick actions
 * for the most common reception flows: open chart or book appointment.
 */
export default function ReceptionPatientSearchPage() {
  const navigate = useNavigate();
  const [term, setTerm] = useState('');
  const [submitted, setSubmitted] = useState('');
  const inputRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  const list = usePatientsList({
    pageNumber: 1,
    pageSize: 25,
    search: submitted || undefined,
    isActive: true,
  });

  const columns: Column<PatientSummary>[] = [
    {
      key: 'mrn',
      header: 'MRN',
      render: (p) => <span className="font-mono text-xs text-gray-800 dark:text-gray-100">{p.mrn}</span>,
    },
    {
      key: 'name',
      header: 'Patient',
      render: (p) => (
        <div>
          <p className="font-medium text-gray-900 dark:text-white">{p.fullName}</p>
          <p className="text-xs text-gray-500">{p.age} yrs · {p.gender}</p>
        </div>
      ),
    },
    { key: 'blood', header: 'Blood', render: (p) => <Badge tone="red">{BLOOD_GROUP_LABEL[p.bloodGroup]}</Badge> },
    { key: 'phone', header: 'Phone', render: (p) => p.phone ?? <span className="text-gray-400">—</span> },
    {
      key: 'actions',
      header: '',
      align: 'right',
      render: (p) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="secondary"
            onClick={(e) => { e.stopPropagation(); navigate(`/patients/${p.id}`); }}
          >
            Open chart
          </Button>
          <Button
            size="sm"
            onClick={(e) => { e.stopPropagation(); navigate(`/appointments/new?patientId=${p.id}`); }}
          >
            Book appointment
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Reception"
        title="Patient search"
        description="Scan an MRN or start typing. Enter to search."
        actions={
          <>
            <Button variant="secondary" onClick={() => navigate('/reception')}>Back to queue</Button>
            <Button onClick={() => navigate('/reception/walk-in')}>Register walk-in</Button>
          </>
        }
      />

      <form
        onSubmit={(e) => {
          e.preventDefault();
          setSubmitted(term.trim());
        }}
        className="flex gap-2"
      >
        <Input
          ref={inputRef}
          placeholder="MRN, name, phone…"
          value={term}
          onChange={(e) => setTerm(e.target.value)}
          className="text-lg"
        />
        <Button type="submit">Search</Button>
        {submitted && (
          <Button type="button" variant="ghost" onClick={() => { setTerm(''); setSubmitted(''); inputRef.current?.focus(); }}>
            Clear
          </Button>
        )}
      </form>

      {list.isError && <Alert variant="error">{humanizeError(list.error)}</Alert>}

      <DataTable
        columns={columns}
        rows={list.data?.items ?? []}
        keyExtractor={(p) => p.id}
        isLoading={list.isPending && !!submitted}
        onRowClick={(p) => navigate(`/patients/${p.id}`)}
        emptyState={
          !submitted
            ? <EmptyState title="Enter a search term" description="Scan or type to find a patient." />
            : <EmptyState
                title="No matches"
                description="Try a different term or register a new patient."
                action={<Button onClick={() => navigate('/reception/walk-in')}>Register walk-in</Button>}
              />
        }
      />
    </div>
  );
}
