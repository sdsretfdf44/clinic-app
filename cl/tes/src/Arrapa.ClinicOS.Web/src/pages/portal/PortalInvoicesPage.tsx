import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  DataTable,
  EmptyState,
  PageHeader,
  Pagination,
  Select,
  type Column,
} from '@/components/ui';
import {
  PORTAL_INVOICE_STATUS_LABEL,
  PORTAL_INVOICE_STATUS_TONE,
  usePortalInvoices,
  type PortalInvoiceStatus,
  type PortalInvoiceSummary,
} from '@/features/portal';

export default function PortalInvoicesPage() {
  const navigate = useNavigate();
  const [status, setStatus] = useState<PortalInvoiceStatus | ''>('');
  const [pageNumber, setPageNumber] = useState(1);

  const params = useMemo(() => ({
    pageNumber,
    pageSize: 20,
    status: status || undefined,
  }), [pageNumber, status]);

  const list = usePortalInvoices(params);

  const columns: Column<PortalInvoiceSummary>[] = [
    {
      key: 'invoice',
      header: 'Invoice',
      render: (i) => (
        <div>
          <p className="text-sm font-mono text-gray-900 dark:text-white">{i.invoiceNumber}</p>
          {i.appointmentNumber && (
            <p className="text-xs text-gray-500 font-mono">{i.appointmentNumber}</p>
          )}
        </div>
      ),
    },
    {
      key: 'issued',
      header: 'Issued',
      render: (i) =>
        i.issuedAt
          ? new Date(i.issuedAt).toLocaleDateString(undefined, { dateStyle: 'medium' })
          : <span className="text-gray-400">—</span>,
    },
    {
      key: 'due',
      header: 'Due',
      render: (i) =>
        i.dueAt
          ? new Date(i.dueAt).toLocaleDateString(undefined, { dateStyle: 'medium' })
          : <span className="text-gray-400">—</span>,
    },
    { key: 'status', header: 'Status', render: (i) => <Badge tone={PORTAL_INVOICE_STATUS_TONE[i.status]}>{PORTAL_INVOICE_STATUS_LABEL[i.status]}</Badge> },
    {
      key: 'total',
      header: 'Total',
      align: 'right',
      render: (i) => `${i.total.toFixed(2)} ${i.currency}`,
    },
    {
      key: 'due',
      header: 'Balance',
      align: 'right',
      render: (i) => (
        <span className={i.amountDue > 0 ? 'text-red-600 dark:text-red-400 font-medium' : 'text-gray-500'}>
          {i.amountDue.toFixed(2)} {i.currency}
        </span>
      ),
    },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Patient portal"
        title="My invoices"
        description="Review outstanding balances and payment history."
      />

      <div className="w-56">
        <label className="text-xs text-gray-500 block mb-1">Status</label>
        <Select value={status} onChange={(e) => { setStatus(e.target.value as PortalInvoiceStatus | ''); setPageNumber(1); }}>
          <option value="">All</option>
          <option value="Issued">Issued</option>
          <option value="PartiallyPaid">Partially paid</option>
          <option value="Paid">Paid</option>
          <option value="Cancelled">Cancelled</option>
        </Select>
      </div>

      {list.isError && <Alert variant="error">{humanizeError(list.error)}</Alert>}

      <DataTable
        columns={columns}
        rows={list.data?.items ?? []}
        keyExtractor={(i) => i.id}
        isLoading={list.isPending}
        onRowClick={(i) => navigate(`/portal/invoices/${i.id}`)}
        emptyState={
          <EmptyState title="No invoices yet" description="Your invoices will appear here once issued." />
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
