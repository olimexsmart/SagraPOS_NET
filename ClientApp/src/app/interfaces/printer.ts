export interface Printer {
    id: number,
    name: string,
    ip: string,
    port: number,
    hidden: boolean
}

export function InitEmptyPrinter() :Printer
{
    return { id: 0, name: 'ND', ip: '0.0.0.0', port: 0, hidden: false }
}
