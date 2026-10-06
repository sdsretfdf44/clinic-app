import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PlusIcon } from '@heroicons/react/24/outline';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Avatar,
  Badge,
  Button,
  DataTable,
  EmptyState,
  FilterBar,
  FilterField,
  FilterSearchInput,
  Input,
  PageHeader,
  Pagination,
  Select,
  type Column,
} from '@/components/ui';
import {
  INVOICE_STATUS_LABEL,
  INVOICE_STATUS_OPTIONS,
  INVOICE_STATUS_TONE,
  useInvoicesList,
  type InvoiceStatus,
  type InvoiceSummary,
} from '@/features/billing';

function toDateInputValue(date: Date) {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

export default function InvoicesListPage() {
  const navigate = useNavigate();

  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [status, setStatus] = useState<InvoiceStatus | ''>('');
  const [search, setSearch] = useState('');
  const [pageNumber, setPageNumber] = useState(1);

  const params = useMemo(() => ({
    pageNumber,
    pageSize: 25,
    from: dateFrom ? new Date(`${dateFrom}T00:00:00`).toISOString() : undefined,
    to: dateTo ? new Date(`${dateTo}T23:59:59`).toISOString() : undefined,
    status: status || undefined,
    search: search || undefined,
  }), [pageNumber, dateFrom, dateTo, status, search]);

  const list = useInvoicesList(params);

  const columns: Column<InvoiceSummary>[] = [
    {
      key: 'invoice',
      header: 'Invoice',
      render: (i) => (
        <div>
          <p className="text-body font-medium text-gray-900 dark:text-white font-mono">{i.invoiceNumber}</p>
          <p className="text-caption text-gray-500">
            {i.issuedAt
              ? new Date(i.issuedAt).toLocaleDateString(undefined, { dateStyle: 'medium' })
              : 'Not issued'}
          </p>
        </div>
      ),
    },
    {
      key: 'patient',
      header: 'Patient',
      render: (i) => (
        <div className="flex items-center gap-3">
          <Avatar name={i.patientName} size="sm" />
          <div className="min-w-0">
            <p className="text-body text-gray-900 dark:text-white truncate">{i.patientName}</p>
            <p className="text-caption text-gray-500 font-mono">{i.patientMrn}</p>
          </div>
        </div>
      ),
    },
    {
      key: 'appointment',
      header: 'Appointment',
      render: (i) =>
        i.appointmentNumber ? (
          <span className="text-caption font-mono text-gray-700 dark:text-gray-200">{i.appointmentNumber}</span>
        ) : (
          <span className="text-gray-400">—</span>
        ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (i) => (
        <Badge tone={INVOICE_STATUS_TONE[i.status]} variant="dot">
          {INVOICE_STATUS_LABEL[i.status]}
        </Badge>
      ),
    },
    {
      key: 'total',
      header: 'Total',
      align: 'right',
      render: (i) => (
        <span className="text-body text-gray-900 dark:text-white tabular-nums">
          {i.total.toFixed(2)} {i.currency}
        </span>
      ),
    },
    {
      key: 'due',
      header: 'Balance',
      align: 'right',
      render: (i) => (
        <span
          className={`text-body tabular-nums ${
            i.amountDue > 0 ? 'text-warning-700 dark:text-warning-500 font-medium' : 'text-gray-400'
          }`}
        >
          {i.amountDue.toFixed(2)} {i.currency}
        </span>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Billing"
        title="Invoices"
        description="Track patient invoices from draft to fully paid."
        actions={
          <Button onClick={() => navigate('/billing/new')} leftIcon={<PlusIcon className="h-4 w-4" />}>
            New invoice
          </Button>
        }
      />

      <FilterBar>
        <FilterSearchInput
          label="Search"
          placeholder="Invoice number, MRN, or patient name"
          value={search}
          onClear={() => {
            setSearch('');
            setPageNumber(1);
          }}
          onChange={(e) => {
            setSearch(e.target.value);
            setPageNumber(1);
          }}
        />
        <FilterField label="From">
          <Input
            type="date"
            value={dateFrom}
            className="h-9"
            onChange={(e) => {
              setDateFrom(e.target.value);
              setPageNumber(1);
            }}
          />
        </FilterField>
        <FilterField label="To">
          <Input
            type="date"
            value={dateTo}
            max={toDateInputValue(new Date())}
            className="h-9"
            onChange={(e) => {
              setDateTo(e.target.value);
              setPageNumber(1);
            }}
          />
        </FilterField>
        <FilterField label="Status">
          <Select
            value={status}
            className="h-9"
            onChange={(e) => {
              setStatus(e.target.value as InvoiceStatus | '');
              setPageNumber(1);
            }}
          >
            <option value="">All statuses</option>
            {INVOICE_STATUS_OPTIONS.map((s) => (
              <option key={s} value={s}>
                {INVOICE_STATUS_LABEL[s]}
              </option>
            ))}
          </Select>
        </FilterField>
      </FilterBar>

      {list.isError && <Alert variant="error">{humanizeError(list.error)}</Alert>}

      <DataTable
        columns={columns}
        rows={list.data?.items ?? []}
        keyExtractor={(i) => i.id}
        isLoading={list.isPending}
        onRowClick={(i) => navigate(`/billing/${i.id}`)}
        emptyState={
          <EmptyState
            title="No invoices"
            description="Create the first invoice for a patient to get started."
            action={
              <Button onClick={() => navigate('/billing/new')} leftIcon={<PlusIcon className="h-4 w-4" />}>
                New invoice
              </Button>
            }
          />
        }
      />

      {list.data && (
        <Pagination
          pageNumber={list.data.pageNumber}
          pageSize={list.data.pageSize}
          totalCount={list.data.totalCount}
          totalPages={list.data.totalPages}
          onPageChange={setPageNumber}
        />
      )}
    </div>
  );
}
