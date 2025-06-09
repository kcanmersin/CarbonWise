import React, { useEffect, useState } from "react";
import Sidebar from "../components/Sidebar";
import { getTestQuestions, startCarbonFootprintTest, saveTestResponse, completeTest } from "../services/carbonFootprintTestService";
import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer } from "recharts";

const menuItems = [
  { key: "dashboard", name: "Dashboard" },
  {
    key: "resource-monitoring",
    name: "Resource Monitoring",
    subItems: [
      { key: "electricity", name: "Electricity" },
      { key: "water", name: "Water" },
      { key: "paper", name: "Paper" },
      { key: "naturalGas", name: "Natural Gas" }
    ]
  },
  {
    key: "carbon-footprint",
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

const COLORS = ["#8884d8", "#82ca9d", "#ffc658", "#ff7f50", "#8dd1e1", "#d084d0"];

const CarbonFootprintTestPage = () => {
  const [questions, setQuestions] = useState([]);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [testId, setTestId] = useState(null);
  const [completedResult, setCompletedResult] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [testStarted, setTestStarted] = useState(false);

  // Load questions when component mounts
  useEffect(() => {
    const loadQuestions = async () => {
      try {
        setLoading(true);
        const questionsData = await getTestQuestions();
        setQuestions(questionsData);
        console.log("Questions loaded:", questionsData);
      } catch (err) {
        console.error("Error loading questions:", err);
        setError("Failed to load questions. Please try again.");
      } finally {
        setLoading(false);
      }
    };

    loadQuestions();
  }, []);

  const handleStartTest = async () => {
    try {
      setLoading(true);
      setError("");
      console.log("Starting carbon footprint test...");
      
      const test = await startCarbonFootprintTest();
      console.log("Test started:", test);
      
      setTestId(test.id || test.testId);
      setTestStarted(true);
      setCurrentQuestionIndex(0);
    } catch (err) {
      console.error("Error starting test:", err);
      setError(err.message || "Failed to start test. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const handleSelectOption = async (optionId) => {
    if (!testId) {
      setError("Test ID is missing. Please start the test again.");
      return;
    }

    const currentQuestion = questions[currentQuestionIndex];
    if (!currentQuestion) {
      setError("Current question not found.");
      return;
    }

    try {
      setLoading(true);
      setError("");
      
      console.log("Saving response:", {
        testId,
        questionId: currentQuestion.id || currentQuestion.questionId,
        optionId
      });

      await saveTestResponse(testId, currentQuestion.id || currentQuestion.questionId, optionId);

      // Check if this was the last question
      if (currentQuestionIndex + 1 < questions.length) {
        setCurrentQuestionIndex(currentQuestionIndex + 1);
      } else {
        // Complete the test
        console.log("Completing test...");
        const result = await completeTest(testId);
        setCompletedResult(result);
      }
    } catch (err) {
      console.error("Error saving response:", err);
      setError(err.message || "Failed to save response. Please try again.");
    } finally {
      setLoading(false);
    }
  };

  const handleRestartTest = () => {
    setTestId(null);
    setTestStarted(false);
    setCurrentQuestionIndex(0);
    setCompletedResult(null);
    setError("");
  };

  const currentQuestion = questions[currentQuestionIndex];

  return (
    <div style={{ display: "flex", backgroundColor: "#f9f9f9", color: "#333", minHeight: "100vh" }}>
      <Sidebar menuItems={menuItems} />
      <div style={{ flex: 1, padding: "2rem", display: "flex", flexDirection: "column", alignItems: "center" }}>
        
        {/* Header */}
        <div style={{ textAlign: "center", marginBottom: "2rem" }}>
          <h1 style={{ fontSize: "2.5rem", fontWeight: "bold", color: "#2c3e50", marginBottom: "0.5rem" }}>
            Carbon Footprint Test
          </h1>
        </div>

        {/* Error Message */}
        {error && (
          <div style={{ 
            color: "#e74c3c", 
            background: "#ffeaea",
            padding: "1rem",
            borderRadius: "8px",
            marginBottom: "1rem",
            border: "1px solid #e74c3c",
            maxWidth: "600px",
            width: "100%"
          }}>
            <strong>Error:</strong> {error}
          </div>
        )}

        {/* Loading Indicator */}
        {loading && (
          <div style={{ 
            display: "flex", 
            alignItems: "center", 
            gap: "1rem", 
            marginBottom: "1rem",
            color: "#4CAF50"
          }}>
            <div style={{
              width: "20px",
              height: "20px",
              border: "2px solid #f3f3f3",
              borderTop: "2px solid #4CAF50",
              borderRadius: "50%",
              animation: "spin 1s linear infinite"
            }} />
            <span>Processing...</span>
          </div>
        )}

        {/* Test Completed */}
        {completedResult ? (
          <div style={{ textAlign: "center", marginTop: "2rem", maxWidth: "800px", width: "100%" }}>
            <h2 style={{ fontSize: "2.5rem", fontWeight: "bold", marginBottom: "1rem", color: "#27ae60" }}>
              🎉 Test Completed!
            </h2>
            <div style={{ 
              background: "white", 
              padding: "2rem", 
              borderRadius: "15px", 
              boxShadow: "0 4px 6px rgba(0,0,0,0.1)",
              marginBottom: "2rem"
            }}>
              <h3 style={{ fontSize: "1.8rem", marginBottom: "2rem", color: "#2c3e50" }}>
                Your Total Carbon Footprint: <span style={{ color: "#e74c3c" }}>{completedResult.totalFootprint}</span> kg CO₂/year
              </h3>

              {completedResult.categoryResults && completedResult.categoryResults.length > 0 && (
                <ResponsiveContainer width="100%" height={400}>
                  <PieChart>
                    <Pie
                      data={completedResult.categoryResults}
                      dataKey="footprintValue"
                      nameKey="category"
                      cx="50%"
                      cy="50%"
                      outerRadius={120}
                      label={({category, footprintValue}) => `${category}: ${footprintValue} kg`}
                    >
                      {completedResult.categoryResults.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip formatter={(value) => [`${value} kg CO₂`, 'Footprint']} />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              )}
            </div>

            <button
              onClick={handleRestartTest}
              style={{
                backgroundColor: "#3498db",
                color: "#fff",
                padding: "1rem 2rem",
                borderRadius: "10px",
                fontSize: "1.2rem",
                border: "none",
                cursor: "pointer",
                transition: "all 0.3s"
              }}
              onMouseOver={(e) => e.target.style.backgroundColor = "#2980b9"}
              onMouseOut={(e) => e.target.style.backgroundColor = "#3498db"}
            >
              Take Test Again
            </button>
          </div>
        ) : (
          /* Test Interface */
          <div style={{ width: "100%", maxWidth: "700px", textAlign: "center" }}>
            {!testStarted ? (
              /* Start Test Button */
              <div style={{ 
                background: "white", 
                padding: "3rem", 
                borderRadius: "15px", 
                boxShadow: "0 4px 6px rgba(0,0,0,0.1)",
                marginTop: "2rem"
              }}>
                <div style={{ fontSize: "4rem", marginBottom: "1rem" }}>🌱</div>
                <h3 style={{ fontSize: "1.5rem", marginBottom: "2rem", color: "#2c3e50" }}>
                  Ready to measure your environmental impact?
                </h3>
                <p style={{ fontSize: "1rem", color: "#7f8c8d", marginBottom: "2rem", lineHeight: "1.6" }}>
                  This test will ask you questions about your daily habits and lifestyle choices 
                  to calculate your personal carbon footprint.
                </p>
                <button
                  onClick={handleStartTest}
                  disabled={loading || questions.length === 0}
                  style={{
                    backgroundColor: loading ? "#95a5a6" : "#4CAF50",
                    color: "#fff",
                    padding: "1.2rem 2.5rem",
                    borderRadius: "12px",
                    fontSize: "1.3rem",
                    border: "none",
                    cursor: loading ? "not-allowed" : "pointer",
                    transition: "all 0.3s",
                    boxShadow: "0 4px 8px rgba(76, 175, 80, 0.3)"
                  }}
                  onMouseOver={(e) => {
                    if (!loading && questions.length > 0) {
                      e.target.style.backgroundColor = "#45a049";
                      e.target.style.transform = "translateY(-2px)";
                    }
                  }}
                  onMouseOut={(e) => {
                    if (!loading && questions.length > 0) {
                      e.target.style.backgroundColor = "#4CAF50";
                      e.target.style.transform = "translateY(0)";
                    }
                  }}
                >
                  {loading ? "Loading..." : questions.length === 0 ? "Loading Questions..." : "🚀 Start Carbon Footprint Test"}
                </button>
              </div>
            ) : (
              /* Question Display */
              currentQuestion && (
                <div style={{ 
                  background: "white", 
                  padding: "2.5rem", 
                  borderRadius: "15px", 
                  boxShadow: "0 4px 6px rgba(0,0,0,0.1)",
                  marginTop: "1rem"
                }}>
                  {/* Progress Bar */}
                  <div style={{ marginBottom: "2rem" }}>
                    <div style={{ 
                      display: "flex", 
                      justifyContent: "space-between", 
                      alignItems: "center", 
                      marginBottom: "0.5rem" 
                    }}>
                      <span style={{ fontSize: "0.9rem", color: "#7f8c8d" }}>
                        Question {currentQuestionIndex + 1} of {questions.length}
                      </span>
                      <span style={{ fontSize: "0.9rem", color: "#7f8c8d" }}>
                        {Math.round(((currentQuestionIndex + 1) / questions.length) * 100)}% Complete
                      </span>
                    </div>
                    <div style={{ 
                      width: "100%", 
                      height: "8px", 
                      backgroundColor: "#ecf0f1", 
                      borderRadius: "4px",
                      overflow: "hidden"
                    }}>
                      <div style={{ 
                        width: `${((currentQuestionIndex + 1) / questions.length) * 100}%`, 
                        height: "100%", 
                        backgroundColor: "#4CAF50",
                        transition: "width 0.3s ease"
                      }} />
                    </div>
                  </div>

                  {/* Category Badge */}
                  {currentQuestion.category && (
                    <div style={{ 
                      display: "inline-block",
                      backgroundColor: "#3498db",
                      color: "white",
                      padding: "0.5rem 1rem",
                      borderRadius: "20px",
                      fontSize: "0.9rem",
                      fontWeight: "600",
                      marginBottom: "1.5rem"
                    }}>
                      📊 {currentQuestion.category}
                    </div>
                  )}

                  {/* Question Text */}
                  <h2 style={{ 
                    fontSize: "1.8rem", 
                    fontWeight: "600", 
                    marginBottom: "1rem",
                    color: "#2c3e50",
                    lineHeight: "1.4"
                  }}>
                    {currentQuestion.text || currentQuestion.questionText}
                  </h2>

                  {/* Question Description */}
                  {currentQuestion.description && (
                    <p style={{ 
                      fontSize: "1rem", 
                      marginBottom: "2rem", 
                      color: "#7f8c8d",
                      lineHeight: "1.6"
                    }}>
                      {currentQuestion.description}
                    </p>
                  )}

                  {/* Answer Options */}
                  <div style={{ 
                    display: "flex", 
                    flexDirection: "column", 
                    gap: "1rem", 
                    width: "100%" 
                  }}>
                    {currentQuestion.options?.map((option, index) => (
                      <button
                        key={option.id}
                        onClick={() => handleSelectOption(option.id)}
                        disabled={loading}
                        style={{
                          backgroundColor: "#fff",
                          color: "#2c3e50",
                          padding: "1.2rem 1.5rem",
                          borderRadius: "12px",
                          border: "2px solid #e9ecef",
                          fontSize: "1.1rem",
                          cursor: loading ? "not-allowed" : "pointer",
                          width: "100%",
                          textAlign: "left",
                          transition: "all 0.3s ease",
                          position: "relative",
                          opacity: loading ? 0.6 : 1
                        }}
                        onMouseOver={(e) => {
                          if (!loading) {
                            e.target.style.backgroundColor = "#f8f9fa";
                            e.target.style.borderColor = "#4CAF50";
                            e.target.style.transform = "translateX(8px)";
                          }
                        }}
                        onMouseOut={(e) => {
                          if (!loading) {
                            e.target.style.backgroundColor = "#fff";
                            e.target.style.borderColor = "#e9ecef";
                            e.target.style.transform = "translateX(0)";
                          }
                        }}
                      >
                        <span style={{ 
                          display: "inline-block", 
                          marginRight: "1rem",
                          width: "24px",
                          height: "24px",
                          borderRadius: "50%",
                          backgroundColor: "#4CAF50",
                          color: "white",
                          textAlign: "center",
                          lineHeight: "24px",
                          fontSize: "0.9rem",
                          fontWeight: "bold"
                        }}>
                          {String.fromCharCode(65 + index)}
                        </span>
                        {option.text || option.optionText}
                      </button>
                    ))}
                  </div>

                  {/* Cancel Test Button */}
                  <div style={{ marginTop: "2rem", textAlign: "center" }}>
                    <button
                      onClick={handleRestartTest}
                      style={{
                        backgroundColor: "transparent",
                        color: "#e74c3c",
                        border: "1px solid #e74c3c",
                        padding: "0.7rem 1.5rem",
                        borderRadius: "8px",
                        fontSize: "0.9rem",
                        cursor: "pointer",
                        transition: "all 0.3s"
                      }}
                      onMouseOver={(e) => {
                        e.target.style.backgroundColor = "#e74c3c";
                        e.target.style.color = "white";
                      }}
                      onMouseOut={(e) => {
                        e.target.style.backgroundColor = "transparent";
                        e.target.style.color = "#e74c3c";
                      }}
                    >
                      Cancel Test
                    </button>
                  </div>
                </div>
              )
            )}
          </div>
        )}

        {/* CSS Animations */}
        <style>
          {`
            @keyframes spin {
              0% { transform: rotate(0deg); }
              100% { transform: rotate(360deg); }
            }
          `}
        </style>
      </div>
    </div>
  );
};
export default CarbonFootprintTestPage;