import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  ArrowLeftIcon,
  BanknotesIcon,
  CheckCircleIcon,
  NoSymbolIcon,
  PrinterIcon,
} from '@heroicons/react/24/outline';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Badge,
  Button,
  Card,
  ConfirmDialog,
  Drawer,
  EmptyState,
  FormField,
  Input,
  PageHeader,
  Select,
  SkeletonCard,
  Textarea,
} from '@/components/ui';
import {
  INVOICE_STATUS_LABEL,
  INVOICE_STATUS_TONE,
  PAYMENT_MODE_LABEL,
  PAYMENT_MODE_OPTIONS,
  useCancelInvoice,
  useIssueInvoice,
  useInvoice,
  useRecordPayment,
  useVoidPayment,
  type PaymentMode,
} from '@/features/billing';

export default function InvoiceDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const invoice = useInvoice(id);

  const issue = useIssueInvoice(id ?? '');
  const cancel = useCancelInvoice(id ?? '');
  const recordPayment = useRecordPayment(id ?? '');
  const voidPayment = useVoidPayment(id ?? '');

  const [showIssue, setShowIssue] = useState(false);
  const [showCancel, setShowCancel] = useState(false);
  const [cancelReason, setCancelReason] = useState('');
  const [showPayment, setShowPayment] = useState(false);

  const [paymentAmount, setPaymentAmount] = useState('');
  const [paymentMethod, setPaymentMethod] = useState<PaymentMode>('Cash');
  const [paymentReference, setPaymentReference] = useState('');
  const [paymentNotes, setPaymentNotes] = useState('');
  const [paymentDate, setPaymentDate] = useState('');
  const [paymentError, setPaymentError] = useState<string | null>(null);

  const [voidTarget, setVoidTarget] = useState<string | null>(null);

  if (invoice.isPending) {
    return (
      <div className="space-y-6">
        <PageHeader eyebrow="Billing" title="Invoice" description="Loading invoice details…" />
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          <SkeletonCard className="lg:col-span-2" lines={4} />
          <SkeletonCard lines={3} />
        </div>
      </div>
    );
  }

  if (invoice.isError || !invoice.data) {
    return (
      <div className="space-y-6">
        <PageHeader eyebrow="Billing" title="Invoice" />
        <Alert variant="error">{humanizeError(invoice.error)}</Alert>
      </div>
    );
  }

  const inv = invoice.data;

  function openPayment() {
    setPaymentError(null);
    setPaymentAmount(inv.amountDue.toFixed(2));
    setPaymentMethod('Cash');
    setPaymentReference('');
    setPaymentNotes('');
    setPaymentDate('');
    setShowPayment(true);
  }

  async function submitPayment() {
    setPaymentError(null);
    const amt = Number(paymentAmount);
    if (!Number.isFinite(amt) || amt <= 0) {
      setPaymentError('Enter a positive amount.');
      return;
    }
    try {
      await recordPayment.mutateAsync({
        amount: amt,
        method: paymentMethod,
        reference: paymentReference.trim() || null,
        paidAt: paymentDate ? new Date(paymentDate).toISOString() : null,
        notes: paymentNotes.trim() || null,
      });
      setShowPayment(false);
    } catch (err) {
      setPaymentError(humanizeError(err));
    }
  }

  async function submitCancel() {
    if (!cancelReason.trim()) return;
    await cancel.mutateAsync(cancelReason.trim());
    setShowCancel(false);
    setCancelReason('');
  }

  async function submitVoid() {
    if (!voidTarget) return;
    await voidPayment.mutateAsync(voidTarget);
    setVoidTarget(null);
  }

  return (
    <div className="space-y-6">
      <PageHeader
        eyebrow={
          <button
            type="button"
            onClick={() => navigate('/billing')}
            className="inline-flex items-center gap-1 text-brand-600 hover:text-brand-700 dark:text-brand-400"
          >
            <ArrowLeftIcon className="h-3.5 w-3.5" /> Back to invoices
          </button>
        }
        title={inv.invoiceNumber}
        description={
          <span className="inline-flex items-center gap-2 flex-wrap">
            <Badge tone={INVOICE_STATUS_TONE[inv.status]} variant="dot">
              {INVOICE_STATUS_LABEL[inv.status]}
            </Badge>
            <span className="text-gray-400">·</span>
            <span>
              {inv.patientName} · <span className="font-mono">{inv.patientMrn}</span>
            </span>
            {inv.appointmentNumber && (
              <>
                <span className="text-gray-400">·</span>
                <button
                  type="button"
                  onClick={() => navigate(`/appointments/${inv.appointmentId}`)}
                  className="text-brand-600 hover:text-brand-700 dark:text-brand-400 font-mono"
                >
                  {inv.appointmentNumber}
                </button>
              </>
            )}
          </span>
        }
        actions={
          <>
            <Button variant="secondary" onClick={() => window.print()} leftIcon={<PrinterIcon className="h-4 w-4" />}>
              Print
            </Button>
            {inv.canIssue && (
              <Button
                variant="secondary"
                onClick={() => setShowIssue(true)}
                isLoading={issue.isPending}
                leftIcon={<CheckCircleIcon className="h-4 w-4" />}
              >
                Issue
              </Button>
            )}
            {inv.canPay && (
              <Button onClick={openPayment} leftIcon={<BanknotesIcon className="h-4 w-4" />}>
                Record payment
              </Button>
            )}
            {inv.canCancel && (
              <Button variant="danger" onClick={() => setShowCancel(true)} leftIcon={<NoSymbolIcon className="h-4 w-4" />}>
                Cancel
              </Button>
            )}
          </>
        }
      />

      {(issue.isError || cancel.isError) && (
        <Alert variant="error">{humanizeError(issue.error ?? cancel.error)}</Alert>
      )}

      {inv.amountDue > 0 && inv.status !== 'Cancelled' && (
        <Alert variant="warning" title="Outstanding balance">
          <span className="tabular-nums font-medium">{inv.amountDue.toFixed(2)} {inv.currency}</span> remaining
          {inv.dueAt && (
            <> · due by {new Date(inv.dueAt).toLocaleDateString(undefined, { dateStyle: 'medium' })}</>
          )}
        </Alert>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <Card padding="lg" className="lg:col-span-2">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-title text-gray-900 dark:text-white">Line items</h3>
          </div>
          {inv.lineItems.length === 0 ? (
            <EmptyState title="No line items" description="Edit this draft to add billable services." />
          ) : (
            <div className="overflow-x-auto -mx-1">
              <table className="min-w-full text-body">
                <thead>
                  <tr className="text-micro uppercase tracking-widest text-gray-500 dark:text-gray-400 border-b border-gray-100 dark:border-gray-800">
                    <th className="text-left py-2.5 pr-3 font-semibold">Description</th>
                    <th className="text-left py-2.5 pr-3 font-semibold">Code</th>
                    <th className="text-right py-2.5 pr-3 font-semibold">Qty</th>
                    <th className="text-right py-2.5 pr-3 font-semibold">Unit</th>
                    <th className="text-right py-2.5 font-semibold">Total</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                  {inv.lineItems.map((l) => (
                    <tr key={l.id}>
                      <td className="py-3 pr-3 text-gray-900 dark:text-white">{l.description}</td>
                      <td className="py-3 pr-3 text-gray-500 font-mono text-caption">{l.serviceCode ?? '—'}</td>
                      <td className="py-3 pr-3 text-right tabular-nums">{l.quantity}</td>
                      <td className="py-3 pr-3 text-right tabular-nums">{l.unitPrice.toFixed(2)}</td>
                      <td className="py-3 text-right tabular-nums font-medium">{l.lineTotal.toFixed(2)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>

        <Card padding="lg" className="space-y-2">
          <h3 className="text-title text-gray-900 dark:text-white mb-3">Summary</h3>
          <SumRow label="Subtotal" value={inv.subtotal} />
          {inv.discountAmount > 0 && <SumRow label="Discount" value={-inv.discountAmount} />}
          {inv.taxAmount > 0 && <SumRow label="Tax" value={inv.taxAmount} />}
          <div className="pt-3 border-t border-gray-100 dark:border-gray-800">
            <SumRow label="Total" value={inv.total} currency={inv.currency} strong />
          </div>
          <SumRow label="Paid" value={inv.amountPaid} tone="success" />
          <SumRow
            label="Balance due"
            value={inv.amountDue}
            currency={inv.currency}
            strong
            tone={inv.amountDue > 0 ? 'warning' : undefined}
          />
          {inv.dueAt && (
            <p className="text-caption text-gray-500 pt-2">
              Due by {new Date(inv.dueAt).toLocaleDateString(undefined, { dateStyle: 'medium' })}
            </p>
          )}
          {inv.notes && (
            <div className="pt-3 border-t border-gray-100 dark:border-gray-800 text-caption text-gray-600 dark:text-gray-300 whitespace-pre-wrap">
              {inv.notes}
            </div>
          )}
          {inv.cancelledAt && (
            <div className="pt-3 border-t border-gray-100 dark:border-gray-800">
              <p className="text-caption text-danger-600 dark:text-danger-500">
                Cancelled {new Date(inv.cancelledAt).toLocaleString()}
              </p>
              {inv.cancellationReason && (
                <p className="text-caption text-gray-500 mt-1">{inv.cancellationReason}</p>
              )}
            </div>
          )}
        </Card>
      </div>

      <Card padding="lg">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-title text-gray-900 dark:text-white">Payments</h3>
          {inv.payments.length > 0 && (
            <p className="text-caption text-gray-500 dark:text-gray-400">
              {inv.payments.length} recorded
            </p>
          )}
        </div>
        {inv.payments.length === 0 ? (
          <EmptyState
            title="No payments recorded"
            description="Record a payment when the patient settles part or all of the invoice."
          />
        ) : (
          <div className="overflow-x-auto -mx-1">
            <table className="min-w-full text-body">
              <thead>
                <tr className="text-micro uppercase tracking-widest text-gray-500 dark:text-gray-400 border-b border-gray-100 dark:border-gray-800">
                  <th className="text-left py-2.5 pr-3 font-semibold">Date</th>
                  <th className="text-left py-2.5 pr-3 font-semibold">Method</th>
                  <th className="text-left py-2.5 pr-3 font-semibold">Reference</th>
                  <th className="text-right py-2.5 pr-3 font-semibold">Amount</th>
                  <th className="text-left py-2.5 pr-3 font-semibold">Notes</th>
                  <th className="py-2.5 w-16"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                {inv.payments.map((p) => (
                  <tr key={p.id}>
                    <td className="py-3 pr-3 text-gray-500 tabular-nums">
                      {new Date(p.paidAt).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })}
                    </td>
                    <td className="py-3 pr-3">
                      <Badge tone="teal">{PAYMENT_MODE_LABEL[p.method]}</Badge>
                    </td>
                    <td className="py-3 pr-3 text-gray-500 font-mono text-caption">{p.reference ?? '—'}</td>
                    <td className="py-3 pr-3 text-right tabular-nums font-medium">{p.amount.toFixed(2)}</td>
                    <td className="py-3 pr-3 text-caption text-gray-600 dark:text-gray-300">{p.notes ?? ''}</td>
                    <td className="py-3 text-right">
                      <button
                        type="button"
                        onClick={() => setVoidTarget(p.id)}
                        className="text-caption text-gray-400 hover:text-danger-600 dark:hover:text-danger-500 transition-colors"
                      >
                        Void
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      <ConfirmDialog
        open={showIssue}
        onClose={() => setShowIssue(false)}
        onConfirm={async () => { await issue.mutateAsync(); setShowIssue(false); }}
        title="Issue this invoice?"
        description="Once issued, line items are locked and the invoice can accept payments."
        confirmLabel="Issue"
        isLoading={issue.isPending}
      />

      <ConfirmDialog
        open={!!voidTarget}
        onClose={() => setVoidTarget(null)}
        onConfirm={submitVoid}
        title="Void this payment?"
        description="The payment will be removed from the total. The row stays for audit."
        confirmLabel="Void payment"
        variant="danger"
        isLoading={voidPayment.isPending}
      />

      <ConfirmDialog
        open={showCancel}
        onClose={() => {
          setShowCancel(false);
          setCancelReason('');
        }}
        onConfirm={submitCancel}
        title="Cancel this invoice?"
        description={
          <div className="space-y-2">
            <p>This action can't be undone.</p>
            <Textarea
              rows={3}
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
              placeholder="Reason for cancellation"
            />
          </div>
        }
        variant="danger"
        confirmLabel="Cancel invoice"
        isLoading={cancel.isPending}
      />

      <Drawer
        open={showPayment}
        onClose={() => setShowPayment(false)}
        title="Record payment"
        description={`Invoice balance: ${inv.amountDue.toFixed(2)} ${inv.currency}`}
        size="md"
        footer={
          <div className="flex items-center justify-end gap-3">
            <Button type="button" variant="secondary" onClick={() => setShowPayment(false)}>
              Cancel
            </Button>
            <Button type="button" onClick={submitPayment} isLoading={recordPayment.isPending}>
              Record payment
            </Button>
          </div>
        }
      >
        <div className="space-y-4">
          {paymentError && <Alert variant="error">{paymentError}</Alert>}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField id="paymentAmount" label="Amount" required>
              <Input
                id="paymentAmount"
                type="number"
                step="0.01"
                min="0"
                max={inv.amountDue}
                value={paymentAmount}
                onChange={(e) => setPaymentAmount(e.target.value)}
              />
            </FormField>
            <FormField id="paymentMethod" label="Method" required>
              <Select
                id="paymentMethod"
                value={paymentMethod}
                onChange={(e) => setPaymentMethod(e.target.value as PaymentMode)}
              >
                {PAYMENT_MODE_OPTIONS.map((m) => (
                  <option key={m} value={m}>
                    {PAYMENT_MODE_LABEL[m]}
                  </option>
                ))}
              </Select>
            </FormField>
            <FormField id="paymentReference" label="Reference">
              <Input
                id="paymentReference"
                value={paymentReference}
                onChange={(e) => setPaymentReference(e.target.value)}
                placeholder="Txn ID, cheque #, UPI ref…"
              />
            </FormField>
            <FormField id="paymentDate" label="Received at">
              <Input
                id="paymentDate"
                type="datetime-local"
                value={paymentDate}
                onChange={(e) => setPaymentDate(e.target.value)}
              />
            </FormField>
          </div>
          <FormField id="paymentNotes" label="Notes">
            <Textarea
              id="paymentNotes"
              rows={3}
              value={paymentNotes}
              onChange={(e) => setPaymentNotes(e.target.value)}
              placeholder="Optional payment notes"
            />
          </FormField>
        </div>
      </Drawer>
    </div>
  );
}

function SumRow({
  label,
  value,
  currency,
  strong,
  tone,
}: {
  label: string;
  value: number;
  currency?: string;
  strong?: boolean;
  tone?: 'success' | 'warning';
}) {
  const valueClass =
    strong ? 'font-semibold text-gray-900 dark:text-white' :
    tone === 'success' ? 'text-success-700 dark:text-success-500' :
    tone === 'warning' ? 'text-warning-700 dark:text-warning-500 font-medium' :
    'text-gray-700 dark:text-gray-200';
  return (
    <div className="flex items-center justify-between text-body">
      <span className="text-gray-500 dark:text-gray-400">{label}</span>
      <span className={`tabular-nums ${valueClass}`}>
        {value.toFixed(2)}
        {currency && <span className="ml-1 text-gray-400">{currency}</span>}
      </span>
    </div>
  );
}
