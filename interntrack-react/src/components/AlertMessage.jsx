function AlertMessage({ message, isError }) {
    if (!message) {
        return null;
    }

    return (
        <p
            style={{
                marginTop: "10px",
                fontWeight: "bold"
            }}
        >
            {isError ? "❌ " : "✅ "}
            {message}
        </p>
    );
}

export default AlertMessage;