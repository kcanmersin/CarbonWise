const AI_URL = "http://localhost:8000";

export const trainModel = async (resourceType, buildingId, modelTypes, ensembleTypes) => {
    const response = await fetch(`${AI_URL}/train`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            "resource_type": resourceType,
            "building_id": buildingId,
            "model_types": modelTypes,
            "ensemble_types": ensembleTypes,
        }),
    });

    if (!response.ok) {
        const err = await response.json();
        throw new Error(err.error || "Training failed");
    }

    return await response.json();
}

export const predict = async (resourceType, buildingId, modelType, monthsAhead) => {
    const response = await fetch(`${AI_URL}/predict`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            "resource_type": resourceType,
            "building_id": buildingId,
            "model_type": modelType,
            "months_ahead": monthsAhead,
        }),
    });

    if (!response.ok) {
        const err = await response.json();
        throw new Error(err.error || "Prediction failed");
    }

    return await response.json();
}

export const getModels = async () => {
    const response = await fetch(`${AI_URL}/models`, {
        method: "GET",
        headers: { "Content-Type": "application/json" },
    });

    if (!response.ok) {
        const err = await response.json();
        throw new Error(err.error || "Failed to fetch models");
    }

    return await response.json();
}

export const getHealth = async () => {
    const response = await fetch(`${AI_URL}/health`, {
        method: "GET",
        headers: { "Content-Type": "application/json" },
    });

    if (!response.ok) {
        const err = await response.json();
        throw new Error(err.error || "Failed to fetch health status");
    }

    return await response.json();
}

export const getBuildingModels = async (resourceType, buildingId) => {
    const response = await fetch(`${AI_URL}/models/${resourceType}/${buildingId}`, {
        method: "GET",
        headers: { "Content-Type": "application/json" },
    });

    if (!response.ok) {
        const err = await response.json();
        throw new Error(err.error || "Failed to fetch building models");
    }

    return await response.json();
}