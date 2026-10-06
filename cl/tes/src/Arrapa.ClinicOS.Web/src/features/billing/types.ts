export type InvoiceStatus =
  | 'Draft'
  | 'Issued'
  | 'PartiallyPaid'
  | 'Paid'
  | 'Cancelled'
  | 'Refunded';

export type PaymentMode =
  | 'Cash'
  | 'Upi'
  | 'CreditCard'
  | 'DebitCard'
  | 'NetBanking'
  | 'Insurance'
  | 'Cheque';

export const INVOICE_STATUS_OPTIONS: InvoiceStatus[] = [
  'Draft',
  'Issued',
  'PartiallyPaid',
  'Paid',
  'Cancelled',
  'Refunded',
];

export const PAYMENT_MODE_OPTIONS: PaymentMode[] = [
  'Cash',
  'Upi',
  'CreditCard',
  'DebitCard',
  'NetBanking',
  'Insurance',
  'Cheque',
];

export const PAYMENT_MODE_LABEL: Record<PaymentMode, string> = {
  Cash: 'Cash',
  Upi: 'UPI',
  CreditCard: 'Credit card',
  DebitCard: 'Debit card',
  NetBanking: 'Net banking',
  Insurance: 'Insurance',
  Cheque: 'Cheque',
};

export const INVOICE_STATUS_LABEL: Record<InvoiceStatus, string> = {
  Draft: 'Draft',
  Issued: 'Issued',
  PartiallyPaid: 'Partially paid',
  Paid: 'Paid',
  Cancelled: 'Cancelled',
  Refunded: 'Refunded',
};

export const INVOICE_STATUS_TONE: Record<InvoiceStatus, 'gray' | 'sky' | 'amber' | 'green' | 'red' | 'brand'> = {
  Draft: 'gray',
  Issued: 'sky',
  PartiallyPaid: 'amber',
  Paid: 'green',
  Cancelled: 'red',
  Refunded: 'brand',
};

export interface InvoiceLineItem {
  id: string;
  description: string;
  serviceCode: string | null;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface InvoicePayment {
  id: string;
  amount: number;
  method: PaymentMode;
  reference: string | null;
  paidAt: string;
  recordedByUserId: string;
  notes: string | null;
}

export interface InvoiceSummary {
  id: string;
  invoiceNumber: string;
  patientId: string;
  patientName: string;
  patientMrn: string;
  appointmentId: string | null;
  appointmentNumber: string | null;
  status: InvoiceStatus;
  issuedAt: string | null;
  dueAt: string | null;
  subtotal: number;
  discountAmount: number;
  taxAmount: number;
  total: number;
  amountPaid: number;
  amountDue: number;
  currency: string;
}

export interface InvoiceDetail extends InvoiceSummary {
  patientPhone: string | null;
  cancelledAt: string | null;
  cancellationReason: string | null;
  notes: string | null;
  lineItems: InvoiceLineItem[];
  payments: InvoicePayment[];
  createdOn: string;
  updatedOn: string | null;
  canEdit: boolean;
  canIssue: boolean;
  canPay: boolean;
  canCancel: boolean;
}
