import { useState } from 'react';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  DataTable,
  Drawer,
  Input,
  PageHeader,
  Pagination,
  Select,
  Spinner,
  type Column,
} from '@/components/ui';
import { useAuditLog, useAuditLogsList, type AuditLog } from '@/features/admin/auditLogs';

const operationTone = (op: string): 'green' | 'amber' | 'red' | 'brand' => {
  switch (op) {
    case 'Created': return 'green';
    case 'Updated': return 'amber';
    case 'Deleted': return 'red';
    default: return 'brand';
  }
};

export default function AuditLogsPage() {
  const [pageNumber, setPageNumber] = useState(1);
  const [search, setSearch] = useState('');
  const [entityType, setEntityType] = useState('');
  const [operation, setOperation] = useState('');
  const [selectedId, setSelectedId] = useState<string | null>(null);

  const params = {
    pageNumber,
    pageSize: 25,
    search: search || undefined,
    entityType: entityType || undefined,
    operation: operation || undefined,
  };
  const list = useAuditLogsList(params);
  const detail = useAuditLog(selectedId ?? undefined);

  const columns: Column<AuditLog>[] = [
    {
      key: 'occurred',
      header: 'Time',
      render: (a) => (
        <div>
          <p className="text-sm text-gray-900 dark:text-white">{new Date(a.occurredOn).toLocaleString()}</p>
          <p className="text-xs text-gray-500 font-mono">{a.correlationId ?? '—'}</p>
        </div>
      ),
    },
    { key: 'op', header: 'Op', render: (a) => <Badge tone={operationTone(a.operation)}>{a.operation}</Badge> },
    {
      key: 'entity',
      header: 'Entity',
      render: (a) => (
        <div>
          <p className="text-sm text-gray-900 dark:text-white">{a.entityType}</p>
          <p className="text-xs text-gray-500 font-mono truncate max-w-[16ch]">{a.entityId}</p>
        </div>
      ),
    },
    { key: 'tenant', header: 'Tenant', render: (a) => a.tenantName ?? <span className="text-gray-400">(platform)</span> },
    { key: 'user', header: 'By', render: (a) => a.performedByEmail ?? <span className="text-gray-400">system</span> },
    { key: 'ip', header: 'IP', render: (a) => a.ipAddress ?? <span className="text-gray-400">—</span> },
  ];

  return (
    <div className="space-y-4">
      <PageHeader
        eyebrow="Super admin"
        title="Audit logs"
        description="Every mutation is captured. Click a row to view before/after values."
      />

      <div className="flex flex-col sm:flex-row gap-3">
        <Input
          placeholder="Search entity id, email, correlation id"
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPageNumber(1);
          }}
        />
        <Input
          placeholder="Entity type (e.g. Tenant)"
          value={entityType}
          onChange={(e) => {
            setEntityType(e.target.value);
            setPageNumber(1);
          }}
        />
        <Select
          className="sm:max-w-xs"
          value={operation}
          onChange={(e) => {
            setOperation(e.target.value);
            setPageNumber(1);
          }}
        >
          <option value="">All operations</option>
          <option value="Created">Created</option>
          <option value="Updated">Updated</option>
          <option value="Deleted">Deleted</option>
        </Select>
      </div>

      {list.isError && <Alert variant="error">{humanizeError(list.error)}</Alert>}

      <DataTable
        columns={columns}
        rows={list.data?.items ?? []}
        keyExtractor={(a) => a.id}
        isLoading={list.isPending}
        onRowClick={(a) => setSelectedId(a.id)}
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

      <Drawer
        open={!!selectedId}
        onClose={() => setSelectedId(null)}
        title="Audit entry"
        size="lg"
        footer={
          <div className="flex justify-end">
            <Button variant="secondary" onClick={() => setSelectedId(null)}>Close</Button>
          </div>
        }
      >
        {detail.isPending && (
          <div className="flex items-center justify-center py-8 text-gray-500">
            <Spinner className="h-5 w-5 mr-2" /> Loading…
          </div>
        )}
        {detail.isError && <Alert variant="error">{humanizeError(detail.error)}</Alert>}
        {detail.data && (
          <div className="space-y-4">
            <dl className="grid grid-cols-1 sm:grid-cols-2 gap-3 text-sm">
              <Field label="Time" value={new Date(detail.data.occurredOn).toLocaleString()} />
              <Field label="Operation" value={<Badge tone={operationTone(detail.data.operation)}>{detail.data.operation}</Badge>} />
              <Field label="Entity" value={`${detail.data.entityType} · ${detail.data.entityId}`} mono />
              <Field label="Tenant" value={detail.data.tenantName ?? '(platform)'} />
              <Field label="Performed by" value={detail.data.performedByEmail ?? 'system'} />
              <Field label="Correlation" value={detail.data.correlationId ?? '—'} mono />
              <Field label="IP" value={detail.data.ipAddress ?? '—'} />
              <Field label="User agent" value={detail.data.userAgent ?? '—'} />
            </dl>
            <div>
              <p className="text-xs uppercase tracking-widest text-gray-400 mb-1">Changes</p>
              <pre className="rounded-lg bg-gray-950 text-gray-100 text-xs p-4 overflow-x-auto whitespace-pre-wrap break-all">
                {detail.data.changesJson ? formatJson(detail.data.changesJson) : '(no diff)'}
              </pre>
            </div>
          </div>
        )}
      </Drawer>
    </div>
  );
}

function Field({ label, value, mono }: { label: string; value: React.ReactNode; mono?: boolean }) {
  return (
    <div>
      <dt className="text-xs uppercase tracking-widest text-gray-400">{label}</dt>
      <dd className={`mt-0.5 text-gray-900 dark:text-white ${mono ? 'font-mono text-xs break-all' : ''}`}>{value}</dd>
    </div>
  );
}

function formatJson(json: string): string {
  try {
    return JSON.stringify(JSON.parse(json), null, 2);
  } catch {
    return json;
  }
}
