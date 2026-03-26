import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import { OrderDetailsResponseDTO } from './userServices/order.service';

@Injectable({ providedIn: 'root' })
export class InvoiceService {

  download(order: OrderDetailsResponseDTO): void {
    const doc = new jsPDF({ orientation: 'portrait', unit: 'mm', format: 'a4' });
    const pageW = doc.internal.pageSize.getWidth();
    const margin = 16;
    const contentW = pageW - margin * 2;
    let y = 0;

    // ── Helpers ──────────────────────────────────────────────────
    const line = (yPos: number) => {
      doc.setDrawColor(229, 231, 235);
      doc.line(margin, yPos, pageW - margin, yPos);
    };

    const text = (
      str: string,
      xPos: number,
      yPos: number,
      opts: { size?: number; bold?: boolean; color?: [number, number, number]; align?: 'left' | 'right' | 'center' } = {}
    ) => {
      doc.setFontSize(opts.size ?? 10);
      doc.setFont('helvetica', opts.bold ? 'bold' : 'normal');
      doc.setTextColor(...(opts.color ?? [55, 65, 81]));
      doc.text(str, xPos, yPos, { align: opts.align ?? 'left' });
    };

    const orderId = order.orderId.split('-')[0].toUpperCase();
    const invoiceDate = new Date().toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });
    const deliveryDate = new Date(order.deliveryDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' });

    // ── Header band ───────────────────────────────────────────────
    doc.setFillColor(29, 78, 216);
    doc.rect(0, 0, pageW, 28, 'F');

    text('ShoppingApp', margin, 11, { size: 16, bold: true, color: [255, 255, 255] });
    text('INVOICE', margin, 20, { size: 10, color: [186, 230, 253] });

    text(`#${orderId}`, pageW - margin, 11, { size: 13, bold: true, color: [255, 255, 255], align: 'right' });
    text(`Date: ${invoiceDate}`, pageW - margin, 18, { size: 9, color: [186, 230, 253], align: 'right' });
    text(`Delivery: ${deliveryDate}`, pageW - margin, 24, { size: 9, color: [186, 230, 253], align: 'right' });

    y = 36;

    // ── Bill To ───────────────────────────────────────────────────
    doc.setFillColor(249, 250, 251);
    doc.roundedRect(margin, y, contentW, 32, 2, 2, 'F');

    text('BILL TO', margin + 4, y + 7, { size: 8, bold: true, color: [107, 114, 128] });
    text(order.orderBy?.userName ?? 'Customer', margin + 4, y + 14, { size: 10, bold: true });
    text(order.orderBy?.userEmail ?? '', margin + 4, y + 20, { size: 9, color: [107, 114, 128] });

    const addr = order.address;
    const addrLine = [addr.addressLine1, addr.addressLine2].filter(Boolean).join(', ');
    const cityLine = `${addr.city}, ${addr.state} - ${addr.pincode}`;
    text(addrLine, margin + 4, y + 26, { size: 9 });
    text(cityLine, margin + 4, y + 31, { size: 9 });

    y += 40;

    // ── Table header ──────────────────────────────────────────────
    doc.setFillColor(29, 78, 216);
    doc.rect(margin, y, contentW, 8, 'F');

    const col = { product: margin + 2, qty: margin + 110, price: margin + 135, total: margin + 162 };

    text('Product', col.product, y + 5.5, { size: 9, bold: true, color: [255, 255, 255] });
    text('Qty', col.qty, y + 5.5, { size: 9, bold: true, color: [255, 255, 255] });
    text('Unit Price', col.price, y + 5.5, { size: 9, bold: true, color: [255, 255, 255] });
    text('Total', col.total, y + 5.5, { size: 9, bold: true, color: [255, 255, 255] });

    y += 8;

    // ── Table rows ────────────────────────────────────────────────
    order.items.forEach((item, i) => {
      const rowH = 9;
      if (i % 2 === 0) {
        doc.setFillColor(249, 250, 251);
        doc.rect(margin, y, contentW, rowH, 'F');
      }

      const name = item.productName.length > 48 ? item.productName.slice(0, 45) + '...' : item.productName;
      const lineTotal = item.quantity * item.productPrice;

      text(name, col.product, y + 6, { size: 9 });
      text(String(item.quantity), col.qty, y + 6, { size: 9 });
      text(`Rs.${item.productPrice.toLocaleString('en-IN')}`, col.price, y + 6, { size: 9 });
      text(`Rs.${lineTotal.toLocaleString('en-IN')}`, col.total, y + 6, { size: 9 });

      y += rowH;
    });

    y += 4;
    line(y);
    y += 6;

    // ── Summary ───────────────────────────────────────────────────
    const summaryX = pageW - margin - 70;

    text('Payment Method:', summaryX, y, { size: 9, color: [107, 114, 128] });
    text(order.payment?.paymentType ?? 'N/A', pageW - margin, y, { size: 9, align: 'right' });
    y += 6;

    text('Total Amount:', summaryX, y, { size: 11, bold: true });
    text(`Rs.${order.totalAmount.toLocaleString('en-IN')}`, pageW - margin, y, { size: 11, bold: true, color: [29, 78, 216], align: 'right' });
    y += 10;

    line(y);
    y += 8;

    // ── Footer note ───────────────────────────────────────────────
    text('Thank you for shopping with us!', pageW / 2, y, { size: 9, color: [107, 114, 128], align: 'center' });
    text('For support: support@shoppingapp.com', pageW / 2, y + 5, { size: 8, color: [156, 163, 175], align: 'center' });

    doc.save(`Invoice_${orderId}.pdf`);
  }
}
