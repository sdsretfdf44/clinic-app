import { Link, useParams } from 'react-router-dom';

import { humanizeError } from '@/api/httpClient';
import { Alert, PageHeader, Spinner } from '@/components/ui';
import { usePortalPrescription } from '@/features/portal';

export default function PortalPrescriptionDetailPage() {
  const { id } = useParams<{ id: string }>();
  const prescription = usePortalPrescription(id);

  if (prescription.isPending) {
    return (
      <div className="flex items-center justify-center py-24 text-gray-500">
        <Spinner className="h-6 w-6 mr-2" /> Loading prescription…
      </div>
    );
  }
  if (prescription.isError || !prescription.data) {
    return <Alert variant="error">{humanizeError(prescription.error)}</Alert>;
  }

  const p = prescription.data;

  return (
    <div className="space-y-6 max-w-4xl">
      <PageHeader
        eyebrow={<Link to="/portal/prescriptions" className="hover:text-brand-600">← Back to prescriptions</Link>}
        title={`Prescription — ${new Date(p.issuedOn).toLocaleDateString(undefined, { dateStyle: 'long' })}`}
        description={
          <span>
            {p.doctorName}
            {p.doctorSpecialty ? ` · ${p.doctorSpecialty}` : ''}
          </span>
        }
      />

      {p.generalInstructions && (
        <Alert variant="info" title="General instructions">
          {p.generalInstructions}
        </Alert>
      )}

      <section className="rounded-2xl border border-gray-100 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 space-y-3">
        <h2 className="text-sm font-semibold text-gray-900 dark:text-white mb-2">Medications</h2>
        {p.items.length === 0 ? (
          <p className="text-sm text-gray-500">No medications listed.</p>
        ) : (
          <ul className="divide-y divide-gray-100 dark:divide-gray-800">
            {p.items.map((item) => (
              <li key={item.id} className="py-3">
                <div className="flex flex-wrap items-baseline gap-2">
                  <p className="text-sm font-medium text-gray-900 dark:text-white">{item.medicine}</p>
                  {item.strength && <span className="text-xs text-gray-500">{item.strength}</span>}
                </div>
                <div className="mt-1 grid grid-cols-1 sm:grid-cols-3 gap-2 text-xs text-gray-600 dark:text-gray-300">
                  {item.dosage && <span><strong>Dosage:</strong> {item.dosage}</span>}
                  {item.frequency && <span><strong>Frequency:</strong> {item.frequency}</span>}
                  {item.duration && <span><strong>Duration:</strong> {item.duration}</span>}
                </div>
                {item.instructions && (
                  <p className="mt-1 text-xs text-gray-500">{item.instructions}</p>
                )}
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}
