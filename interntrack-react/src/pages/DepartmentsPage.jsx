import { useEffect, useState } from "react";
import { api } from "../services/api";
import AlertMessage from "../components/AlertMessage";
import LoadingMessage from "../components/LoadingMessage";
import { getErrorMessage } from "../utils/getErrorMessage";

function DepartmentsPage() {
    const [departments, setDepartments] = useState([]);
    const [name, setName] = useState("");

    const [message, setMessage] = useState("");
    const [isError, setIsError] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isLoading, setIsLoading] = useState(true);

    async function loadDepartments() {
        setIsLoading(true);
        setMessage("");
        setIsError(false);

        try {
            const data = await api.get("/departments");
            setDepartments(data ?? []);
        } catch (error) {
            setIsError(true);
            setMessage(getErrorMessage(error));
        } finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        loadDepartments();
    }, []);

    async function handleSubmit(event) {
        event.preventDefault();

        setMessage("");
        setIsError(false);
        setIsSubmitting(true);

        const newDepartment = {
            name
        };

        try {
            const data = await api.post(
                "/departments",
                newDepartment
            );

            setIsError(false);
            setMessage(
                data?.message || "Departman oluşturuldu."
            );

            setName("");

            await loadDepartments();
        } catch (error) {
            setIsError(true);
            setMessage(getErrorMessage(error));
        } finally {
            setIsSubmitting(false);
        }
    }

    async function deleteDepartment(id) {
        setMessage("");
        setIsError(false);

        try {
            await api.delete(`/departments/${id}`);

            setIsError(false);
            setMessage("Departman başarıyla silindi.");

            await loadDepartments();
        } catch (error) {
            setIsError(true);
            setMessage(getErrorMessage(error));
        }
    }

    return (
        <div>
            <h2>Departmanlar</h2>

            <form onSubmit={handleSubmit}>
                <input
                    type="text"
                    placeholder="Departman Adı"
                    value={name}
                    onChange={e => setName(e.target.value)}
                    required
                />

                <button
                    type="submit"
                    disabled={isSubmitting}
                >
                    {isSubmitting
                        ? "Ekleniyor..."
                        : "Departman Ekle"}
                </button>
            </form>

            <AlertMessage
                message={message}
                isError={isError}
            />

            <h3>Departman Listesi</h3>

            {isLoading ? (
                <LoadingMessage />
            ) : departments.length === 0 ? (
                <p>Henüz departman yok.</p>
            ) : (
                departments.map(department => (
                    <div
                        className="department-card"
                        key={department.id}
                    >
                        {department.name}

                        <button
                            onClick={() =>
                                deleteDepartment(department.id)
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

export default DepartmentsPage;