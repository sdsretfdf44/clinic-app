import {
  deleteResource,
  getPaginated,
  getResource,
  postResource,
  putResource,
} from '@/features/admin/shared';

import type { InvoiceDetail, InvoiceSummary } from './types';

export interface InvoiceLineItemPayload {
  description: string;
  serviceCode: string | null;
  quantity: number;
  unitPrice: number;
}

export interface CreateInvoicePayload {
  patientId: string;
  appointmentId: string | null;
  dueAt: string | null;
  discountAmount: number;
  taxAmount: number;
  currency: string | null;
  notes: string | null;
  lineItems: InvoiceLineItemPayload[];
}

export interface UpdateInvoicePayload {
  dueAt: string | null;
  discountAmount: number;
  taxAmount: number;
  notes: string | null;
  lineItems: InvoiceLineItemPayload[];
}

export interface RecordPaymentPayload {
  amount: number;
  method: string;
  reference: string | null;
  paidAt: string | null;
  notes: string | null;
}

interface ListParams {
  pageNumber: number;
  pageSize: number;
  patientId?: string;
  status?: string;
  from?: string;
  to?: string;
  search?: string;
}

export const invoicesApi = {
  list: (params: ListParams) => getPaginated<InvoiceSummary>('/v1/billing/invoices', params),
  get: (id: string) => getResource<InvoiceDetail>(`/v1/billing/invoices/${id}`),
  create: (payload: CreateInvoicePayload) =>
    postResource<CreateInvoicePayload, InvoiceDetail>('/v1/billing/invoices', payload),
  update: (id: string, payload: UpdateInvoicePayload) =>
    putResource<UpdateInvoicePayload, InvoiceDetail>(`/v1/billing/invoices/${id}`, payload),
  issue: (id: string) => postResource<unknown, unknown>(`/v1/billing/invoices/${id}/issue`, {}),
  cancel: (id: string, reason: string) =>
    postResource<{ reason: string }, unknown>(`/v1/billing/invoices/${id}/cancel`, { reason }),
  remove: (id: string) => deleteResource(`/v1/billing/invoices/${id}`),
  recordPayment: (id: string, payload: RecordPaymentPayload) =>
    postResource<RecordPaymentPayload, InvoiceDetail>(`/v1/billing/invoices/${id}/payments`, payload),
  voidPayment: (id: string, paymentId: string) =>
    deleteResource(`/v1/billing/invoices/${id}/payments/${paymentId}`),
};
