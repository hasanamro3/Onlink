using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.IO;

namespace Onlink.ML
{
    // 📥 الإدخال العام للنموذج (من السيرة والوصف)
    public class ModelInput
    {
        public string ResumeText { get; set; }
        public string JobDescription { get; set; }
    }

    // 📤 إخراج التنبؤ
    public class ModelOutput
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Score { get; set; }
    }

    // 📊 بيانات التدريب من ملف CSV
    public class ResumeData
    {
        [LoadColumn(0)] public string ResumeText { get; set; }
        [LoadColumn(1)] public string JobDescription { get; set; }
        [LoadColumn(2), ColumnName("Label")] public bool IsMatch { get; set; }
    }

    // 🔍 نموذج التنبؤ الاختباري (اختياري للتجارب)
    public class ResumePrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Score { get; set; }
    }

    // 🔧 تدريب سريع (تشغيل يدوي فقط)
    public static class MLTrainer
    {
        public static void TrainAndSaveModel()
        {
            var mlContext = new MLContext();
            var dataPath = Path.Combine(AppContext.BaseDirectory, "Data", "CV_Job_Match_LargeDataset.csv");

            if (!File.Exists(dataPath))
            {
                Console.WriteLine($"❌ ملف البيانات غير موجود: {dataPath}");
                return;
            }

            var data = mlContext.Data.LoadFromTextFile<ResumeData>(
                path: dataPath,
                separatorChar: ',',
                hasHeader: true
            );

            var pipeline = mlContext.Transforms.Text.FeaturizeText("ResumeFeaturized", nameof(ResumeData.ResumeText))
                .Append(mlContext.Transforms.Text.FeaturizeText("JobFeaturized", nameof(ResumeData.JobDescription)))
                .Append(mlContext.Transforms.Concatenate("Features", "ResumeFeaturized", "JobFeaturized"))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

            var model = pipeline.Fit(data);
            mlContext.Model.Save(model, data.Schema, "MLModel.zip");

            Console.WriteLine("✅ النموذج تم تدريبه وحفظه في MLModel.zip");
        }
    }

    // 🤖 تنبؤ ثابت باستخدام النموذج
    public static class MLModel
    {
        private static readonly string _modelPath = "MLModel.zip";
        private static readonly MLContext _mlContext = new MLContext();
        private static readonly PredictionEngine<ModelInput, ModelOutput> _predictionEngine;

        static MLModel()
        {
            if (!File.Exists(_modelPath))
            {
                Console.WriteLine($"❌ النموذج غير موجود: {_modelPath}");
                return;
            }

            var model = _mlContext.Model.Load(_modelPath, out _);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
        }

        public static ModelOutput Predict(ModelInput input)
        {
            return _predictionEngine.Predict(input);
        }
    }

    // 🧩 خدمة تدريب مثل DataContext (قابلة للإضافة إلى DI)
    public class MLService
    {
        private readonly MLContext _mlContext;

        public MLService()
        {
            _mlContext = new MLContext();
        }

        public void TrainAndSaveModel()
        {
            var dataPath = Path.Combine(AppContext.BaseDirectory, "Data", "CV_Job_Match_LargeDataset.csv");

            if (!File.Exists(dataPath))
            {
                Console.WriteLine($"❌ ملف البيانات غير موجود: {dataPath}");
                return;
            }

            var data = _mlContext.Data.LoadFromTextFile<ResumeData>(
                path: dataPath,
                separatorChar: ',',
                hasHeader: true
            );

            var pipeline = _mlContext.Transforms.Text.FeaturizeText("ResumeFeaturized", nameof(ResumeData.ResumeText))
                .Append(_mlContext.Transforms.Text.FeaturizeText("JobFeaturized", nameof(ResumeData.JobDescription)))
                .Append(_mlContext.Transforms.Concatenate("Features", "ResumeFeaturized", "JobFeaturized"))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

            var model = pipeline.Fit(data);
            _mlContext.Model.Save(model, data.Schema, "MLModel.zip");

            Console.WriteLine("✅ تم تدريب النموذج وحفظه من خلال MLService");
        }
    }
}
