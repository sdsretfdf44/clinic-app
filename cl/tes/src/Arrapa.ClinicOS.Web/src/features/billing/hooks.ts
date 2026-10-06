import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';

import {
  invoicesApi,
  type CreateInvoicePayload,
  type RecordPaymentPayload,
  type UpdateInvoicePayload,
} from './api';

const keys = {
  root: ['billing'] as const,
  invoices: ['billing', 'invoices'] as const,
  invoiceList: (p: unknown) => ['billing', 'invoices', 'list', p] as const,
  invoice: (id: string) => ['billing', 'invoices', 'detail', id] as const,
};

export function useInvoicesList(params: Parameters<typeof invoicesApi.list>[0]) {
  return useQuery({
    queryKey: keys.invoiceList(params),
    queryFn: () => invoicesApi.list(params),
    staleTime: 15_000,
  });
}

export function useInvoice(id: string | undefined) {
  return useQuery({
    queryKey: id ? keys.invoice(id) : ['billing', 'invoices', 'detail', 'none'],
    queryFn: () => invoicesApi.get(id!),
    enabled: !!id,
  });
}

export function useCreateInvoice() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateInvoicePayload) => invoicesApi.create(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.invoices }),
  });
}

export function useUpdateInvoice(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: UpdateInvoicePayload) => invoicesApi.update(id, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.invoice(id) });
      qc.invalidateQueries({ queryKey: keys.invoices });
    },
  });
}

export function useIssueInvoice(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: () => invoicesApi.issue(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.invoice(id) });
      qc.invalidateQueries({ queryKey: keys.invoices });
    },
  });
}

export function useCancelInvoice(id: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (reason: string) => invoicesApi.cancel(id, reason),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.invoice(id) });
      qc.invalidateQueries({ queryKey: keys.invoices });
    },
  });
}

export function useDeleteInvoice() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => invoicesApi.remove(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.invoices }),
  });
}

export function useRecordPayment(invoiceId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (payload: RecordPaymentPayload) => invoicesApi.recordPayment(invoiceId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.invoice(invoiceId) });
      qc.invalidateQueries({ queryKey: keys.invoices });
    },
  });
}

export function useVoidPayment(invoiceId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (paymentId: string) => invoicesApi.voidPayment(invoiceId, paymentId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: keys.invoice(invoiceId) });
      qc.invalidateQueries({ queryKey: keys.invoices });
    },
  });
}
