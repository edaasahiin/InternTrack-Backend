import { useState } from "react";

function InternList({ interns, onInternDeleted }) {
    const [message, setMessage] = useState("");
    const [isError, setIsError] = useState(false);

    async function deleteIntern(id) {
        setMessage("");
        setIsError(false);

        try {
            const response = await fetch(
                `http://localhost:5053/api/interns/${id}`,
                {
                    method: "DELETE"
                }
            );

            if (!response.ok) {
                let data = null;

                try {
                    data = await response.json();
                } catch {
                    data = null;
                }

                setIsError(true);
                setMessage(
                    data?.message || "Stajyer silinemedi."
                );

                return;
            }

            setIsError(false);
            setMessage("Stajyer başarıyla silindi.");

            onInternDeleted();
        } catch (error) {
            console.error(error);

            setIsError(true);
            setMessage("Sunucuya bağlanılamadı.");
        }
    }

    return (
        <div>
            <h3>Stajyer Listesi</h3>

            {message && (
                <p
                    style={{
                        marginTop: "10px",
                        fontWeight: "bold"
                    }}
                >
                    {isError ? "❌ " : "✅ "}
                    {message}
                </p>
            )}

            {interns.length === 0 ? (
                <p>Henüz stajyer yok.</p>
            ) : (
                interns.map(intern => (
                    <div
                        className="intern-card"
                        key={intern.id}
                    >
                        <strong>{intern.name}</strong>
                        {" - "}
                        {intern.email}
                        {" - "}
                        {intern.department?.name}

                        <button
                            onClick={() =>
                                deleteIntern(intern.id)
                            }
                        >
                            Sil
                        </button>
                    </div>
                ))
            )}
        </div>
    );
}

export default InternList;