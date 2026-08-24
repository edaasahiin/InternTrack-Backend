import { useState } from "react";
import { api } from "../services/api";
import AlertMessage from "./AlertMessage";
import { getErrorMessage } from "../utils/getErrorMessage";

function InternList({ interns, onInternDeleted }) {
    const [message, setMessage] = useState("");
    const [isError, setIsError] = useState(false);

    async function deleteIntern(id) {
        setMessage("");
        setIsError(false);

        try {
            await api.delete(`/interns/${id}`);

            setIsError(false);
            setMessage("Stajyer başarıyla silindi.");

            onInternDeleted();
        } catch (error) {
            setIsError(true);
            setMessage(getErrorMessage(error));
        }
    }

    return (
        <div>
            <h3>Stajyer Listesi</h3>

            <AlertMessage
                message={message}
                isError={isError}
            />

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