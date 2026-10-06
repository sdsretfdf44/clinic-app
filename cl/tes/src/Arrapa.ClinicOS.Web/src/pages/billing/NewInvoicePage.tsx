import { useMemo, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import {
  Alert,
  Button,
  FormField,
  Input,
  PageHeader,
  Select,
  Textarea,
} from '@/components/ui';
import { useCreateInvoice } from '@/features/billing';
import { usePatientsList } from '@/features/patients/hooks';

interface LineDraft {
  description: string;
  serviceCode: string;
  quantity: string;
  unitPrice: string;
}

function emptyLine(): LineDraft {
  return { description: '', serviceCode: '', quantity: '1', unitPrice: '0' };
}

function toNumber(v: string) {
  const n = Number(v);
  return Number.isFinite(n) ? n : 0;
}

export default function NewInvoicePage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const createInvoice = useCreateInvoice();

  const [patientId, setPatientId] = useState(searchParams.get('patientId') ?? '');
  const [appointmentId, setAppointmentId] = useState(searchParams.get('appointmentId') ?? '');
  const [dueAt, setDueAt] = useState('');
  const [discount, setDiscount] = useState('0');
  const [tax, setTax] = useState('0');
  const [currency, setCurrency] = useState('');
  const [notes, setNotes] = useState('');
  const [patientSearch, setPatientSearch] = useState('');
  const [lines, setLines] = useState<LineDraft[]>([emptyLine()]);
  const [error, setError] = useState<string | null>(null);

  const patients = usePatientsList({
    pageNumber: 1,
    pageSize: 25,
    search: patientSearch || undefined,
    isActive: true,
  });

  const subtotal = useMemo(
    () => lines.reduce((sum, l) => sum + toNumber(l.quantity) * toNumber(l.unitPrice), 0),
    [lines],
  );
  const total = Math.max(0, subtotal - toNumber(discount) + toNumber(tax));

  function updateLine(idx: number, patch: Partial<LineDraft>) {
    setLines((prev) => prev.map((l, i) => (i === idx ? { ...l, ...patch } : l)));
  }

  function addLine() {
    setLines((prev) => [...prev, emptyLine()]);
  }

  function removeLine(idx: number) {
    setLines((prev) => (prev.length === 1 ? prev : prev.filter((_, i) => i !== idx)));
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);

    if (!patientId) {
      setError('Please pick a patient.');
      return;
    }
    const cleanLines = lines
      .map((l) => ({
        description: l.description.trim(),
        serviceCode: l.serviceCode.trim() || null,
        quantity: toNumber(l.quantity),
        unitPrice: toNumber(l.unitPrice),
      }))
      .filter((l) => l.description.length > 0 && l.quantity > 0);
    if (cleanLines.length === 0) {
      setError('Add at least one line item with a description and quantity.');
      return;
    }

    try {
      const invoice = await createInvoice.mutateAsync({
        patientId,
        appointmentId: appointmentId || null,
        dueAt: dueAt ? new Date(dueAt).toISOString() : null,
        discountAmount: toNumber(discount),
        taxAmount: toNumber(tax),
        currency: currency.trim() || null,
        notes: notes.trim() || null,
        lineItems: cleanLines,
      });
      navigate(`/billing/${invoice.id}`);
    } catch (err) {
      setError(humanizeError(err));
    }
  }

  return (
    <div className="max-w-4xl">
      <PageHeader
        eyebrow="Billing"
        title="New invoice"
        description="Draft an invoice with one or more line items. You can issue and collect payment after review."
      />

      <form onSubmit={onSubmit} className="space-y-4" noValidate>
        {error && <Alert variant="error">{error}</Alert>}

        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField id="patientSearch" label="Patient search">
              <Input
                id="patientSearch"
                placeholder="MRN, name, phone"
                value={patientSearch}
                onChange={(e) => setPatientSearch(e.target.value)}
              />
            </FormField>
            <FormField id="patientId" label="Patient" required>
              <Select id="patientId" value={patientId} onChange={(e) => setPatientId(e.target.value)}>
                <option value="">Select patient…</option>
                {patients.data?.items.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.fullName} — {p.mrn}
                  </option>
                ))}
              </Select>
            </FormField>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <FormField id="appointmentId" label="Appointment ID (optional)">
              <Input
                id="appointmentId"
                placeholder="Leave blank for ad-hoc invoice"
                value={appointmentId}
                onChange={(e) => setAppointmentId(e.target.value)}
              />
            </FormField>
            <FormField id="dueAt" label="Due date">
              <Input id="dueAt" type="date" value={dueAt} onChange={(e) => setDueAt(e.target.value)} />
            </FormField>
            <FormField id="currency" label="Currency (override)">
              <Input
                id="currency"
                placeholder="Uses tenant default"
                maxLength={10}
                value={currency}
                onChange={(e) => setCurrency(e.target.value)}
              />
            </FormField>
          </div>
        </section>

        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 space-y-3">
          <div className="flex items-center justify-between">
            <h3 className="text-sm font-semibold text-gray-900 dark:text-white">Line items</h3>
            <Button type="button" size="sm" variant="secondary" onClick={addLine}>
              + Add line
            </Button>
          </div>

          <div className="overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead>
                <tr className="text-xs uppercase tracking-widest text-gray-400">
                  <th className="text-left py-2 pr-3 font-normal">Description</th>
                  <th className="text-left py-2 pr-3 font-normal">Code</th>
                  <th className="text-right py-2 pr-3 font-normal">Qty</th>
                  <th className="text-right py-2 pr-3 font-normal">Unit price</th>
                  <th className="text-right py-2 pr-3 font-normal">Total</th>
                  <th className="py-2 w-8"></th>
                </tr>
              </thead>
              <tbody>
                {lines.map((line, idx) => {
                  const lineTotal = toNumber(line.quantity) * toNumber(line.unitPrice);
                  return (
                    <tr key={idx} className="border-t border-gray-100 dark:border-gray-800 align-top">
                      <td className="py-2 pr-3">
                        <Input
                          placeholder="e.g. Consultation fee"
                          value={line.description}
                          onChange={(e) => updateLine(idx, { description: e.target.value })}
                        />
                      </td>
                      <td className="py-2 pr-3 w-32">
                        <Input
                          placeholder="—"
                          value={line.serviceCode}
                          onChange={(e) => updateLine(idx, { serviceCode: e.target.value })}
                        />
                      </td>
                      <td className="py-2 pr-3 w-24">
                        <Input
                          type="number"
                          step="0.01"
                          min="0"
                          className="text-right"
                          value={line.quantity}
                          onChange={(e) => updateLine(idx, { quantity: e.target.value })}
                        />
                      </td>
                      <td className="py-2 pr-3 w-32">
                        <Input
                          type="number"
                          step="0.01"
                          min="0"
                          className="text-right"
                          value={line.unitPrice}
                          onChange={(e) => updateLine(idx, { unitPrice: e.target.value })}
                        />
                      </td>
                      <td className="py-2 pr-3 text-right tabular-nums">{lineTotal.toFixed(2)}</td>
                      <td className="py-2 text-right">
                        <button
                          type="button"
                          onClick={() => removeLine(idx)}
                          disabled={lines.length === 1}
                          className="text-gray-400 hover:text-red-500 disabled:opacity-40 disabled:cursor-not-allowed"
                          aria-label="Remove line"
                        >
                          ✕
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </section>

        <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-6">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
            <div className="space-y-4">
              <FormField id="discount" label="Discount">
                <Input id="discount" type="number" step="0.01" min="0" value={discount} onChange={(e) => setDiscount(e.target.value)} />
              </FormField>
              <FormField id="tax" label="Tax">
                <Input id="tax" type="number" step="0.01" min="0" value={tax} onChange={(e) => setTax(e.target.value)} />
              </FormField>
              <FormField id="notes" label="Notes">
                <Textarea id="notes" rows={3} value={notes} onChange={(e) => setNotes(e.target.value)} />
              </FormField>
            </div>
            <div className="rounded-lg bg-gray-50 dark:bg-gray-800/60 p-4 text-sm space-y-2">
              <div className="flex justify-between"><span className="text-gray-500">Subtotal</span><span className="tabular-nums">{subtotal.toFixed(2)}</span></div>
              <div className="flex justify-between"><span className="text-gray-500">Discount</span><span className="tabular-nums">-{toNumber(discount).toFixed(2)}</span></div>
              <div className="flex justify-between"><span className="text-gray-500">Tax</span><span className="tabular-nums">+{toNumber(tax).toFixed(2)}</span></div>
              <div className="border-t border-gray-200 dark:border-gray-700 pt-2 flex justify-between font-semibold">
                <span>Total</span><span className="tabular-nums">{total.toFixed(2)}</span>
              </div>
            </div>
          </div>
        </section>

        <div className="flex items-center justify-end gap-3">
          <Button type="button" variant="secondary" onClick={() => navigate(-1)}>Cancel</Button>
          <Button type="submit" isLoading={createInvoice.isPending}>Create draft</Button>
        </div>
      </form>
    </div>
  );
}
