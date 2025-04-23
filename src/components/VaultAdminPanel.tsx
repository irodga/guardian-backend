// src/components/VaultAdminPanel.tsx
import { useEffect, useState } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

export default function VaultAdminPanel() {
  const [path, setPath] = useState("guardian/token/master");
  const [value, setValue] = useState("");
  const [cas, setCas] = useState(0);
  const [response, setResponse] = useState<string | null>(null);

  async function fetchSecret() {
    const res = await fetch(`/vault/secret/${path}`);
    if (!res.ok) return setResponse("No se pudo leer el secreto.");
    const data = await res.json();
    setValue(data.value);
    setResponse("Secreto leído correctamente.");
  }

  async function writeSecret() {
    const res = await fetch(`/vault/secret/${path}`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ value, cas }),
    });
    const data = await res.json();
    if (!res.ok) return setResponse("Error al guardar secreto.");
    setResponse("Secreto guardado con éxito.");
  }

  return (
    <div className="max-w-xl mx-auto mt-10 space-y-4">
      <h1 className="text-xl font-bold">Panel Admin - Vault Secrets</h1>
      <Input value={path} onChange={(e) => setPath(e.target.value)} placeholder="Ruta del secreto (ej. guardian/token/master)" />
      <Input value={value} onChange={(e) => setValue(e.target.value)} placeholder="Valor del secreto" />
      <Input type="number" value={cas} onChange={(e) => setCas(parseInt(e.target.value))} placeholder="CAS (versión esperada)" />
      <div className="flex gap-2">
        <Button onClick={fetchSecret}>Leer Secreto</Button>
        <Button onClick={writeSecret}>Guardar Secreto</Button>
      </div>
      {response && <Card><CardContent className="p-4 text-sm">{response}</CardContent></Card>}
    </div>
  );
}
