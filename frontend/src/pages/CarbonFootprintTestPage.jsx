import React, { useEffect, useState } from "react";
import Sidebar from "../components/Sidebar";
import { getTestQuestions, startCarbonFootprintTest, saveTestResponse, completeTest } from "../services/carbonFootprintTestService";
import { PieChart, Pie, Cell, Tooltip, Legend } from "recharts";

const menuItems = [
  { key: "dashboard", name: "Dashboard" },
  {
    key: "resourceMonitoring",
    name: "Resource Monitoring",
    subItems: [
      { key: "electricity", name: "Electricity" },
      { key: "water", name: "Water" },
      { key: "paper", name: "Paper" },
      { key: "naturalGas", name: "Natural Gas" }
    ]
  },
  {
    key: "carbonFootprint",
    name: "Carbon Footprint",
    subItems: [
      { key: "test", name: "Test" },
      { key: "calculations", name: "Calculations" }
    ]
  },
  { key: "predictions", name: "Predictions" },
  { key: "adminTools", name: "Admin Tools" },
  { key: "reports", name: "Reports" }
];

const COLORS = ["#8884d8", "#82ca9d", "#ffc658", "#ff7f50"];

const CarbonFootprintTestPage = () => {
  const [questions, setQuestions] = useState([]);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [testId, setTestId] = useState(null);
  const [completedResult, setCompletedResult] = useState(null);
  const [error, setError] = useState("");

  useEffect(() => {
    const loadQuestions = async () => {
      try {
        const questionsData = await getTestQuestions();
        setQuestions(questionsData);
      } catch (err) {
        console.error(err);
        setError("Failed to load questions.");
      }
    };
    loadQuestions();
  }, []);

  const handleStartTest = async () => {
    try {
      const test = await startCarbonFootprintTest();
      setTestId(test.id || test.testId);
    } catch (err) {
      console.error(err);
      setError("Failed to start test.");
    }
  };

  const handleSelectOption = async (optionId) => {
    if (!testId) return;
    const currentQuestion = questions[currentQuestionIndex];

    try {
      await saveTestResponse(testId, currentQuestion.id || currentQuestion.questionId, optionId);
      if (currentQuestionIndex + 1 < questions.length) {
        setCurrentQuestionIndex(currentQuestionIndex + 1);
      } else {
        const result = await completeTest(testId);
        setCompletedResult(result);
      }
    } catch (err) {
      console.error(err);
      setError("Failed to save response.");
    }
  };

  const currentQuestion = questions[currentQuestionIndex];

  return (
    <div style={{ display: "flex", backgroundColor: "#f9f9f9", color: "#333", minHeight: "100vh" }}>
      <Sidebar menuItems={menuItems} />
      <div style={{ flex: 1, padding: "2rem", display: "flex", flexDirection: "column", alignItems: "center" }}>
        {error && <div style={{ color: "red", marginBottom: "1rem" }}>{error}</div>}

        {completedResult ? (
          <div style={{ textAlign: "center", marginTop: "2rem" }}>
            <h2 style={{ fontSize: "2rem", fontWeight: "bold", marginBottom: "1rem" }}>Test Completed!</h2>
            <h3 style={{ fontSize: "1.5rem", marginBottom: "2rem" }}>
              Total Carbon Footprint: {completedResult.totalFootprint} kg CO₂
            </h3>

            <PieChart width={400} height={400}>
              <Pie
                data={completedResult.categoryResults}
                dataKey="footprintValue"
                nameKey="category"
                cx="50%"
                cy="50%"
                outerRadius={150}
                label
              >
                {completedResult.categoryResults.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip />
              <Legend />
            </PieChart>
          </div>
        ) : (
          <div style={{ width: "100%", maxWidth: "600px", textAlign: "center", marginTop: "3rem" }}>
            {!testId ? (
              <button
                onClick={handleStartTest}
                style={{
                  backgroundColor: "#4CAF50",
                  color: "#fff",
                  padding: "1rem 2rem",
                  borderRadius: "10px",
                  fontSize: "1.5rem",
                  border: "none",
                  cursor: "pointer",
                  marginTop: "5rem"
                }}
              >
                Start Carbon Footprint Test
              </button>
            ) : (
              <>
                {currentQuestion && (
                  <>
                    {/* Category */}
                    <div style={{ fontSize: "1.8rem", fontWeight: "bold", color: "#ff9800", marginBottom: "1rem" }}>
                      {currentQuestion.category || "Category"}
                    </div>

                    {/* Question */}
                    <div style={{ fontSize: "2rem", fontWeight: "bold", marginBottom: "1rem" }}>
                      {currentQuestion.text || currentQuestion.questionText}
                    </div>

                    {/* Explanation */}
                    {currentQuestion.description && (
                      <div style={{ fontSize: "1rem", marginBottom: "2rem", color: "#666" }}>
                        {currentQuestion.description}
                      </div>
                    )}

                    {/* Options */}
                    <div style={{ display: "flex", flexDirection: "column", gap: "1rem", width: "100%" }}>
                      {currentQuestion.options?.map((option) => (
                        <button
                          key={option.id}
                          onClick={() => handleSelectOption(option.id)}
                          style={{
                            backgroundColor: "#fff",
                            color: "#333",
                            padding: "1rem",
                            borderRadius: "8px",
                            border: "1px solid #ccc",
                            fontSize: "1.2rem",
                            cursor: "pointer",
                            width: "100%",
                            boxShadow: "0px 2px 5px rgba(0, 0, 0, 0.1)"
                          }}
                          onMouseOver={(e) => e.target.style.backgroundColor = "#f0f0f0"}
                          onMouseOut={(e) => e.target.style.backgroundColor = "#fff"}
                        >
                          {option.text || option.optionText}
                        </button>
                      ))}
                    </div>
                  </>
                )}
              </>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default CarbonFootprintTestPage;
