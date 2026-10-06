import { Link, useParams } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import { Alert, Badge, PageHeader, Spinner } from '@/components/ui';
import {
  PORTAL_INVOICE_STATUS_LABEL,
  PORTAL_INVOICE_STATUS_TONE,
  usePortalInvoice,
} from '@/features/portal';

export default function PortalInvoiceDetailPage() {
  const { id } = useParams<{ id: string }>();
  const invoice = usePortalInvoice(id);

  if (invoice.isPending) {
    return (
      <div className="flex items-center justify-center py-24 text-gray-500">
        <Spinner className="h-6 w-6 mr-2" /> Loading invoice…
      </div>
    );
  }
  if (invoice.isError || !invoice.data) {
    return <Alert variant="error">{humanizeError(invoice.error)}</Alert>;
  }

  const i = invoice.data;

  return (
    <div className="space-y-6 max-w-4xl">
      <PageHeader
        eyebrow={<Link to="/portal/invoices" className="hover:text-brand-600">← Back to invoices</Link>}
        title={i.invoiceNumber}
        description={
          <span className="flex items-center gap-2 flex-wrap">
            <Badge tone={PORTAL_INVOICE_STATUS_TONE[i.status]}>{PORTAL_INVOICE_STATUS_LABEL[i.status]}</Badge>
            {i.appointmentNumber && (
              <span className="text-xs text-gray-500 font-mono">Appointment: {i.appointmentNumber}</span>
            )}
          </span>
        }
      />

      {i.amountDue > 0 && i.status !== 'Cancelled' && (
        <Alert variant="warning">
          Balance due: <strong>{i.amountDue.toFixed(2)} {i.currency}</strong>
          {i.dueAt && <> · Due by {new Date(i.dueAt).toLocaleDateString(undefined, { dateStyle: 'medium' })}</>}
        </Alert>
      )}

      <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 space-y-4">
        <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Line items</h2>
        {i.lineItems.length === 0 ? (
          <p className="text-sm text-gray-500">No line items.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-left text-xs uppercase tracking-widest text-gray-400 border-b border-gray-100 dark:border-gray-800">
                  <th className="pb-2 pr-3">Description</th>
                  <th className="pb-2 pr-3 text-right">Qty</th>
                  <th className="pb-2 pr-3 text-right">Unit price</th>
                  <th className="pb-2 text-right">Total</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                {i.lineItems.map((line) => (
                  <tr key={line.id}>
                    <td className="py-2 pr-3">
                      <p className="text-gray-900 dark:text-gray-100">{line.description}</p>
                      {line.serviceCode && <p className="text-xs text-gray-500 font-mono">{line.serviceCode}</p>}
                    </td>
                    <td className="py-2 pr-3 text-right text-gray-700 dark:text-gray-200">{line.quantity}</td>
                    <td className="py-2 pr-3 text-right text-gray-700 dark:text-gray-200">{line.unitPrice.toFixed(2)}</td>
                    <td className="py-2 text-right text-gray-900 dark:text-gray-100">{line.lineTotal.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5">
        <div className="max-w-sm ml-auto space-y-1 text-sm">
          <SumRow label="Subtotal" value={i.subtotal} currency={i.currency} />
          {i.discountAmount > 0 && <SumRow label="Discount" value={-i.discountAmount} currency={i.currency} />}
          {i.taxAmount > 0 && <SumRow label="Tax" value={i.taxAmount} currency={i.currency} />}
          <SumRow label="Total" value={i.total} currency={i.currency} strong />
          <SumRow label="Paid" value={i.amountPaid} currency={i.currency} />
          <SumRow label="Balance" value={i.amountDue} currency={i.currency} strong />
        </div>
      </section>

      {i.payments.length > 0 && (
        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 space-y-3">
          <h2 className="text-sm font-semibold text-gray-900 dark:text-white">Payments</h2>
          <ul className="divide-y divide-gray-100 dark:divide-gray-800">
            {i.payments.map((p) => (
              <li key={p.id} className="py-2 flex items-center justify-between text-sm">
                <div>
                  <p className="text-gray-900 dark:text-gray-100">{p.amount.toFixed(2)} {i.currency}</p>
                  <p className="text-xs text-gray-500">
                    {p.method}
                    {p.reference ? ` · Ref: ${p.reference}` : ''}
                  </p>
                </div>
                <p className="text-xs text-gray-500">{new Date(p.paidAt).toLocaleString()}</p>
              </li>
            ))}
          </ul>
        </section>
      )}

      {i.status === 'Cancelled' && i.cancellationReason && (
        <Alert variant="warning" title="Cancelled">
          Reason: {i.cancellationReason}
        </Alert>
      )}

      {i.notes && (
        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5">
          <p className="text-xs uppercase tracking-widest text-gray-400 mb-2">Notes</p>
          <p className="text-sm text-gray-700 dark:text-gray-200 whitespace-pre-line">{i.notes}</p>
        </section>
      )}
    </div>
  );
}

function SumRow({ label, value, currency, strong }: { label: string; value: number; currency: string; strong?: boolean }) {
  return (
    <div className={`flex justify-between ${strong ? 'font-semibold text-gray-900 dark:text-white' : 'text-gray-600 dark:text-gray-300'}`}>
      <span>{label}</span>
      <span>{value.toFixed(2)} {currency}</span>
    </div>
  );
}
