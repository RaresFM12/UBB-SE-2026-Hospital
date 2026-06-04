namespace Hospital.Shared.Repositories
{
    using System.Collections.Generic;
    using Hospital.Shared.Models;

    public interface IEvaluationsRepository
    {
        IReadOnlyList<MedicalEvaluation> GetAllEvaluations();

        void AddEvaluation(int doctorId, int patientId, string diagnosis, string notes, string medications, bool assumedRisk);

        void UpdateEvaluation(int evaluationId, string diagnosis, string notes, string medications);

        void DeleteEvaluation(int evaluationId);
    }
}
