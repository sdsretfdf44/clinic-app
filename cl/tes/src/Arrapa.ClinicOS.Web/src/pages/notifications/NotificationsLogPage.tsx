import { useState } from 'react';
import { PaperAirplaneIcon } from '@heroicons/react/24/outline';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  type BadgeTone,
  Card,
  DataTable,
  type Column,
  EmptyState,
  Input,
  PageHeader,
  Pagination,
  Select,
} from '@/components/ui';
import { useNotificationLogs, type NotificationLog } from '@/features/notifications';

const KINDS = ['', 'BookingConfirmation', 'Reminder24h', 'Reminder1h', 'ManualResend'];
const CHANNELS = ['', 'WhatsApp', 'Sms', 'Email'];
const STATUSES = ['', 'Pending', 'Sent', 'Failed', 'Skipped'];

export default function NotificationsLogPage() {
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize] = useState(25);
  const [kind, setKind] = useState('');
  const [channel, setChannel] = useState('');
  const [status, setStatus] = useState('');
  const [search, setSearch] = useState('');

  const list = useNotificationLogs({
    pageNumber,
    pageSize,
    kind: kind || undefined,
    channel: channel || undefined,
    status: status || undefined,
    search: search || undefined,
  });

  const columns: Column<NotificationLog>[] = [
    {
      key: 'when',
      header: 'When',
      render: (r) => (
        <div className="text-caption">
          <p className="text-gray-900 dark:text-white">{formatDate(r.sentAt ?? r.createdOn)}</p>
          {r.sentAt && r.sentAt !== r.createdOn && (
            <p className="text-gray-500">Queued {formatDate(r.createdOn)}</p>
          )}
        </div>
      ),
    },
    {
      key: 'kind',
      header: 'Kind',
      render: (r) => <Badge tone="sky">{r.kind}</Badge>,
    },
    {
      key: 'channel',
      header: 'Channel',
      render: (r) => <Badge tone="brand" variant="outline">{r.channel}</Badge>,
    },
    {
      key: 'to',
      header: 'To',
      render: (r) => (
        <div>
          <p className="text-body text-gray-900 dark:text-white">{r.recipient}</p>
          {r.patientName && <p className="text-caption text-gray-500">{r.patientName}</p>}
        </div>
      ),
    },
    {
      key: 'body',
      header: 'Message',
      render: (r) => (
        <p className="text-caption text-gray-600 dark:text-gray-300 line-clamp-2 max-w-md">{r.body}</p>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (r) => (
        <div>
          <Badge tone={statusTone(r.status)}>{r.status}</Badge>
          {r.errorMessage && (
            <p className="text-caption text-error-600 dark:text-error-400 mt-1 line-clamp-2 max-w-xs">{r.errorMessage}</p>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow="Notifications"
        title="Notification log"
        description="Every WhatsApp / SMS reminder dispatched by the clinic. Booking confirmations, 24h reminders, 1h reminders, and manual resends."
      />

      <Card padding="md">
        <div className="grid grid-cols-2 md:grid-cols-5 gap-3">
          <Select value={kind} onChange={(e) => { setPageNumber(1); setKind(e.target.value); }} aria-label="Kind">
            {KINDS.map((k) => <option key={k} value={k}>{k || 'All kinds'}</option>)}
          </Select>
          <Select value={channel} onChange={(e) => { setPageNumber(1); setChannel(e.target.value); }} aria-label="Channel">
            {CHANNELS.map((c) => <option key={c} value={c}>{c || 'All channels'}</option>)}
          </Select>
          <Select value={status} onChange={(e) => { setPageNumber(1); setStatus(e.target.value); }} aria-label="Status">
            {STATUSES.map((s) => <option key={s} value={s}>{s || 'All statuses'}</option>)}
          </Select>
          <div className="col-span-2">
            <Input
              type="search"
              placeholder="Search recipient or message body…"
              value={search}
              onChange={(e) => { setPageNumber(1); setSearch(e.target.value); }}
            />
          </div>
        </div>
      </Card>

      {list.isError && <Alert variant="error">{humanizeError(list.error)}</Alert>}

      <DataTable<NotificationLog>
        columns={columns}
        rows={list.data?.items ?? []}
        keyExtractor={(r) => r.id}
        isLoading={list.isPending}
        emptyState={
          <EmptyState
            icon={<PaperAirplaneIcon className="h-6 w-6" />}
            title="No notifications yet"
            description="Once the reminder scheduler sends a WhatsApp or SMS message, it will appear here."
          />
        }
      />

      {list.data && list.data.totalCount > 0 && (
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

function statusTone(status: string): BadgeTone {
  switch (status) {
    case 'Sent': return 'green';
    case 'Failed': return 'red';
    case 'Skipped': return 'gray';
    case 'Pending':
    default: return 'amber';
  }
}

function formatDate(iso: string): string {
  try {
    return new Date(iso).toLocaleString();
  } catch {
    return iso;
  }
}
