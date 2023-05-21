export interface InfoOrders
{
    infoOrderEntries: InfoOrderEntry[],
    ordersTotal: number,
    numOrders: number
}

interface InfoOrderEntry
{
    menuEntryName: string,
    quantitySold: number,
    totalSold: number,
    totalSoldPercentage: number,
    totalPercentage: number,
}