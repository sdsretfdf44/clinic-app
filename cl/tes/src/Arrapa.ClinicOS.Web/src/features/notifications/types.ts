export interface NotificationLog {
  id: string;
  appointmentId: string | null;
  appointmentNumber: string | null;
  patientName: string | null;
  kind: string;
  channel: string;
  status: string;
  recipient: string;
  body: string;
  providerMessageId: string | null;
  errorMessage: string | null;
  sentAt: string | null;
  createdOn: string;
}
